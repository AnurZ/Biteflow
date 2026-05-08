using Market.Application.Abstractions;
using Market.Application.Modules.Meal.Queries.GetList;
using Market.Domain.Entities.Catalog;
using Market.Shared.Constants;
using Microsoft.Extensions.Time.Testing;

namespace Market.Tests.AuthTests;

public sealed class TenantFilteringTests
{
    [Fact]
    public async Task GlobalFilter_ShouldIsolateTenantRows_PerContextInstance()
    {
        var dbName = Guid.NewGuid().ToString();
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        await using (var seed = CreateContext(dbName, new TestTenantContext(tenantA)))
        {
            seed.ProductCategories.AddRange(
                new ProductCategoryEntity { Name = "Tenant A", TenantId = tenantA, IsEnabled = true },
                new ProductCategoryEntity { Name = "Tenant B", TenantId = tenantB, IsEnabled = true });
            await seed.SaveChangesAsync();
        }

        await using var tenantAContext = CreateContext(dbName, new TestTenantContext(tenantA));
        await using var tenantBContext = CreateContext(dbName, new TestTenantContext(tenantB));

        var tenantAItems = await tenantAContext.ProductCategories.Select(x => x.Name).ToListAsync();
        var tenantBItems = await tenantBContext.ProductCategories.Select(x => x.Name).ToListAsync();

        Assert.Equal(["Tenant A"], tenantAItems);
        Assert.Equal(["Tenant B"], tenantBItems);
    }

    [Fact]
    public async Task GlobalFilter_ShouldAllowSuperAdminToReadAllTenantRows()
    {
        var dbName = Guid.NewGuid().ToString();
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        await using (var seed = CreateContext(dbName, new TestTenantContext(tenantA)))
        {
            seed.ProductCategories.AddRange(
                new ProductCategoryEntity { Name = "Tenant A", TenantId = tenantA, IsEnabled = true },
                new ProductCategoryEntity { Name = "Tenant B", TenantId = tenantB, IsEnabled = true });
            await seed.SaveChangesAsync();
        }

        await using var superAdminContext = CreateContext(dbName, new TestTenantContext(null, null, true));

        var names = await superAdminContext.ProductCategories
            .OrderBy(x => x.Name)
            .Select(x => x.Name)
            .ToListAsync();

        Assert.Equal(["Tenant A", "Tenant B"], names);
    }

    [Fact]
    public async Task RestaurantScopedQuery_ShouldFilterWithinCurrentTenantRestaurant()
    {
        var dbName = Guid.NewGuid().ToString();
        var tenantId = Guid.NewGuid();
        var restaurantA = Guid.NewGuid();
        var restaurantB = Guid.NewGuid();

        await using (var seed = CreateContext(dbName, new TestTenantContext(tenantId, restaurantA)))
        {
            seed.Meals.AddRange(
                new Market.Domain.Entities.Meal.Meal
                {
                    Name = "Visible Meal",
                    TenantId = tenantId,
                    RestaurantId = restaurantA,
                    IsAvailable = true
                },
                new Market.Domain.Entities.Meal.Meal
                {
                    Name = "Other Restaurant Meal",
                    TenantId = tenantId,
                    RestaurantId = restaurantB,
                    IsAvailable = true
                });
            await seed.SaveChangesAsync();
        }

        await using var db = CreateContext(dbName, new TestTenantContext(tenantId, restaurantA));
        var handler = new GetMealsQueryHandler(db, new TestTenantContext(tenantId, restaurantA));

        var result = await handler.Handle(new GetMealsQuery(), CancellationToken.None);

        Assert.Equal(1, result.Total);
        Assert.Equal("Visible Meal", Assert.Single(result.Items).Name);
    }

    [Fact]
    public void ProductionIgnoreQueryFilters_ShouldRemainLimitedToDocumentedSystemFlows()
    {
        var allowedProductionFiles = new[]
        {
            "Backend/Market.Application/Modules/TenantActivation",
            "Backend/Market.Application/Modules/TableReservation/Commands/CreateTableReservation/CreatePublicTableReservationCommandHandler.cs",
            "Backend/Market.Infrastructure/Common/PublicTenantResolver.cs",
            "Backend/Market.Infrastructure/Common/ActivationLinkService.cs",
            "Backend/Market.Infrastructure/Identity/StaffProfileService.cs",
            "Backend/Market.Infrastructure/Database/Seeders"
        };

        var productionFiles = Directory
            .EnumerateFiles(GetBackendPath(), "*.cs", SearchOption.AllDirectories)
            .Where(path => !Normalize(path).Contains("/Market.Tests/"))
            .Where(path => File.ReadAllText(path).Contains("IgnoreQueryFilters()"))
            .Select(Normalize)
            .ToArray();

        Assert.All(productionFiles, file =>
            Assert.Contains(allowedProductionFiles, allowed => file.Contains(allowed)));
    }

    private static DatabaseContext CreateContext(string dbName, ITenantContext tenantContext)
    {
        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        return new DatabaseContext(options, new FakeTimeProvider(), tenantContext);
    }

    private static string GetBackendPath()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !Directory.Exists(Path.Combine(directory.FullName, "Backend")))
        {
            directory = directory.Parent;
        }

        if (directory is null)
        {
            throw new DirectoryNotFoundException("Backend directory was not found.");
        }

        return Path.Combine(directory.FullName, "Backend");
    }

    private static string Normalize(string path) => path.Replace('\\', '/');

    private sealed class TestTenantContext(
        Guid? tenantId,
        Guid? restaurantId = null,
        bool isSuperAdmin = false) : ITenantContext
    {
        public Guid? TenantId => tenantId;
        public Guid? RestaurantId => restaurantId ?? SeedConstants.DefaultRestaurantId;
        public bool IsSuperAdmin => isSuperAdmin;
    }
}
