using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Market.Application.Abstractions;
using Market.Application.Modules.TableLayout.Commands.UpdateTableLayout;
using Market.Application.Modules.TableLayout.Querries.GetTableLayouts;
using Market.Domain.Common.Enums;
using Market.Domain.Entities.DiningTables;
using Market.Domain.Entities.TableLayout;
using Market.Domain.Entities.Tenants;
using Market.Domain.Entities.IdentityV2;
using Market.Shared.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Market.Tests.AuthTests;

public sealed class ControllerAuthorizationIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private const string CaptchaBypassToken = "10000000-aaaa-bbbb-cccc-000000000001";
    private const string DemoRestaurantDomain = "demo-bistro-restaurant";

    private readonly CustomWebApplicationFactory<Program> _factory;

    public ControllerAuthorizationIntegrationTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Theory]
    [InlineData("waiter1", "waiter1", "/api/TableLayout")]
    [InlineData("string", "string", "/api/Analytics/revenue-per-day")]
    [InlineData("string", "string", "/api/inventoryitem")]
    public async Task StaffOrAdmin_ShouldAccessRepresentativeProtectedEndpoints(
        string username,
        string password,
        string url)
    {
        var client = await _factory.GetAuthenticatedClientAsync(username, password);

        var response = await client.GetAsync(url);

        response.EnsureSuccessStatusCode();
    }

    [Theory]
    [MemberData(nameof(CustomerForbiddenRequests))]
    public async Task Customer_ShouldBeForbiddenFromStaffAndAdminEndpoints(Func<HttpClient, Task<HttpResponseMessage>> send)
    {
        var client = await CreateCustomerClientAsync();

        var response = await send(client);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Theory]
    [InlineData("/api/Analytics/revenue-per-day")]
    [InlineData("/api/TableReservation")]
    public async Task Anonymous_ShouldBeRejectedFromProtectedEndpoints(string url)
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync(url);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("not-a-guid")]
    public async Task StaffTokenWithoutValidTenantClaim_ShouldBeForbiddenFromTenantScopedEndpoints(string? tenantClaim)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", BuildUnsignedJwt(tenantClaim));

        var response = await client.GetAsync("/api/TableReservation");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AnonymousCustomerRegistration_WithKnownDomain_ShouldCreateCustomerInResolvedTenant()
    {
        var email = $"customer-{Guid.NewGuid():N}@example.test";
        var client = CreateClientForDomain(DemoRestaurantDomain);

        var response = await PostCustomerRegistrationAsync(client, email);

        response.EnsureSuccessStatusCode();

        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.FindByEmailAsync(email);

        Assert.NotNull(user);
        Assert.Equal(SeedConstants.DefaultTenantId, user!.TenantId);
        Assert.Equal(SeedConstants.DefaultRestaurantId, user.RestaurantId);
    }

    [Fact]
    public async Task AnonymousCustomerRegistration_WithUnknownDomain_ShouldReturnBadRequest()
    {
        var client = CreateClientForDomain("unknown-restaurant.test");

        var response = await PostCustomerRegistrationAsync(client, $"customer-{Guid.NewGuid():N}@example.test");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PublicReservation_WithKnownDomain_ShouldCreateReservationInResolvedTenant()
    {
        var client = CreateClientForDomain(DemoRestaurantDomain);
        var startsAt = DateTime.UtcNow.AddDays(20).AddMinutes(7);

        var response = await client.PostAsJsonAsync("/api/public/table-reservations", new
        {
            DiningTableId = 1,
            NumberOfGuests = 2,
            ReservationStart = startsAt,
            ReservationEnd = startsAt.AddHours(1),
            FirstName = "Public",
            LastName = "Guest",
            Email = $"reservation-{Guid.NewGuid():N}@example.test",
            PhoneNumber = "123"
        });

        response.EnsureSuccessStatusCode();
        var reservationId = await response.Content.ReadFromJsonAsync<int>();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        var reservation = await db.TableReservations
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == reservationId);

        Assert.NotNull(reservation);
        Assert.Equal(SeedConstants.DefaultTenantId, reservation!.TenantId);
    }

    [Fact]
    public async Task PublicReservation_ShouldRejectDiningTableFromAnotherTenant()
    {
        var otherTableId = await CreateDiningTableForOtherTenantAsync();
        var client = CreateClientForDomain(DemoRestaurantDomain);
        var startsAt = DateTime.UtcNow.AddDays(25).AddMinutes(11);

        var response = await client.PostAsJsonAsync("/api/public/table-reservations", new
        {
            DiningTableId = otherTableId,
            NumberOfGuests = 2,
            ReservationStart = startsAt,
            ReservationEnd = startsAt.AddHours(1),
            FirstName = "Public",
            LastName = "Guest",
            Email = $"reservation-{Guid.NewGuid():N}@example.test",
            PhoneNumber = "123"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Anonymous_ShouldCreateUpdateReadAndSubmitTenantActivationRequest()
    {
        var client = _factory.CreateClient();
        var domain = $"join-{Guid.NewGuid():N}";

        var createResponse = await client.PostAsJsonAsync("/api/activation-requests", new
        {
            RestaurantName = "Join Test Bistro",
            Domain = domain,
            OwnerFullName = "Join Owner",
            OwnerEmail = $"join-{Guid.NewGuid():N}@example.test",
            OwnerPhone = "123456",
            Address = "Join Address",
            City = "Mostar",
            State = "FBIH"
        });

        createResponse.EnsureSuccessStatusCode();
        var requestId = await createResponse.Content.ReadFromJsonAsync<int>();

        var getResponse = await client.GetAsync($"/api/activation-requests/{requestId}");
        getResponse.EnsureSuccessStatusCode();
        var draft = await getResponse.Content.ReadFromJsonAsync<ActivationDraftDto>();

        Assert.NotNull(draft);
        Assert.Equal(ActivationStatus.Draft, draft!.Status);

        var updatedDomain = $"{domain}-updated";
        var updateResponse = await client.PutAsJsonAsync($"/api/activation-requests/{requestId}", new
        {
            Id = requestId,
            RestaurantName = "Updated Join Test Bistro",
            Domain = updatedDomain,
            OwnerFullName = "Join Owner",
            OwnerEmail = $"join-updated-{Guid.NewGuid():N}@example.test",
            OwnerPhone = "654321",
            Address = "Updated Join Address",
            City = "Sarajevo",
            State = "FBIH"
        });
        updateResponse.EnsureSuccessStatusCode();

        var submitResponse = await client.PostAsync($"/api/activation-requests/{requestId}/submit", null);
        submitResponse.EnsureSuccessStatusCode();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        var request = await db.TenantActivationRequests
            .IgnoreQueryFilters()
            .FirstAsync(x => x.Id == requestId);

        Assert.Equal(Guid.Empty, request.TenantId);
        Assert.Equal(updatedDomain, request.Domain);
        Assert.Equal(ActivationStatus.Submitted, request.Status);
    }

    [Fact]
    public async Task Admin_ShouldNotUpdateOrDeleteTableLayoutFromAnotherRestaurant()
    {
        var otherLayoutId = await CreateTableLayoutForOtherTenantAsync();
        var client = await _factory.GetAuthenticatedClientAsync();

        var updateResponse = await client.PutAsJsonAsync($"/api/TableLayout/{otherLayoutId}", new
        {
            Name = $"Cross Tenant Layout {Guid.NewGuid():N}",
            BackgroundColor = "#ffffff",
            FloorImageUrl = string.Empty
        });
        var deleteResponse = await client.DeleteAsync($"/api/TableLayout/{otherLayoutId}");
        var getNameResponse = await client.GetAsync($"/api/TableLayout/{otherLayoutId}/name");

        Assert.Equal(HttpStatusCode.NotFound, updateResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, getNameResponse.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        var layout = await db.TableLayouts
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == otherLayoutId);

        Assert.NotNull(layout);
    }

    [Fact]
    public async Task Admin_ShouldNotUpdateDeleteOrReadTableLayoutFromAnotherRestaurantInSameTenant()
    {
        var otherLayoutId = await CreateTableLayoutForSameTenantOtherRestaurantAsync();
        var client = await _factory.GetAuthenticatedClientAsync();

        var updateResponse = await client.PutAsJsonAsync($"/api/TableLayout/{otherLayoutId}", new
        {
            Name = $"Same Tenant Cross Restaurant Layout {Guid.NewGuid():N}",
            BackgroundColor = "#ffffff",
            FloorImageUrl = string.Empty
        });
        var deleteResponse = await client.DeleteAsync($"/api/TableLayout/{otherLayoutId}");
        var getNameResponse = await client.GetAsync($"/api/TableLayout/{otherLayoutId}/name");

        Assert.Equal(HttpStatusCode.NotFound, updateResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, getNameResponse.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        var layout = await db.TableLayouts
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == otherLayoutId);

        Assert.NotNull(layout);
    }

    [Fact]
    public async Task Admin_ShouldNotAssignDiningTableToLayoutFromAnotherRestaurant()
    {
        var otherLayoutId = await CreateTableLayoutForOtherTenantAsync();
        var client = await _factory.GetAuthenticatedClientAsync();

        var createResponse = await client.PostAsJsonAsync("/api/DiningTable", new
        {
            Number = Random.Shared.Next(10_000, 99_999),
            NumberOfSeats = 4,
            TableType = 0,
            TableLayoutId = otherLayoutId,
            X = 0,
            Y = 0,
            Width = 100,
            Height = 100,
            Shape = "rectangle",
            Color = "#ffffff",
            Status = 0
        });

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        var existingTable = await db.DiningTables
            .IgnoreQueryFilters()
            .FirstAsync(x => x.TenantId == SeedConstants.DefaultTenantId);
        var originalLayoutId = existingTable.TableLayoutId;

        var updateResponse = await client.PutAsJsonAsync($"/api/DiningTable/{existingTable.Id}", new
        {
            existingTable.Number,
            existingTable.NumberOfSeats,
            existingTable.IsActive,
            TableLayoutId = otherLayoutId,
            existingTable.X,
            existingTable.Y,
            existingTable.Height,
            existingTable.Width,
            existingTable.Shape,
            existingTable.Color,
            existingTable.TableType,
            existingTable.Status,
            existingTable.LastUsedAt
        });

        await db.Entry(existingTable).ReloadAsync();

        Assert.Equal(HttpStatusCode.NotFound, createResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, updateResponse.StatusCode);
        Assert.Equal(originalLayoutId, existingTable.TableLayoutId);
    }

    [Fact]
    public async Task Admin_ShouldNotAssignDiningTableToLayoutFromAnotherRestaurantInSameTenant()
    {
        var otherLayoutId = await CreateTableLayoutForSameTenantOtherRestaurantAsync();
        var client = await _factory.GetAuthenticatedClientAsync();

        var createResponse = await client.PostAsJsonAsync("/api/DiningTable", new
        {
            Number = Random.Shared.Next(10_000, 99_999),
            NumberOfSeats = 4,
            TableType = 0,
            TableLayoutId = otherLayoutId,
            X = 0,
            Y = 0,
            Width = 100,
            Height = 100,
            Shape = "rectangle",
            Color = "#ffffff",
            Status = 0
        });

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        var existingTable = await db.DiningTables
            .IgnoreQueryFilters()
            .FirstAsync(x => x.TenantId == SeedConstants.DefaultTenantId);
        var originalLayoutId = existingTable.TableLayoutId;

        var updateResponse = await client.PutAsJsonAsync($"/api/DiningTable/{existingTable.Id}", new
        {
            existingTable.Number,
            existingTable.NumberOfSeats,
            existingTable.IsActive,
            TableLayoutId = otherLayoutId,
            existingTable.X,
            existingTable.Y,
            existingTable.Height,
            existingTable.Width,
            existingTable.Shape,
            existingTable.Color,
            existingTable.TableType,
            existingTable.Status,
            existingTable.LastUsedAt
        });

        await db.Entry(existingTable).ReloadAsync();

        Assert.Equal(HttpStatusCode.NotFound, createResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, updateResponse.StatusCode);
        Assert.Equal(originalLayoutId, existingTable.TableLayoutId);
    }

    [Fact]
    public async Task UpdateTableLayout_ShouldNotUseTrackedLayoutFromAnotherRestaurant()
    {
        var tenantId = SeedConstants.DefaultTenantId;
        var currentRestaurantId = SeedConstants.DefaultRestaurantId;
        var otherRestaurantId = Guid.NewGuid();
        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var db = new DatabaseContext(
            options,
            new Microsoft.Extensions.Time.Testing.FakeTimeProvider(),
            new TestTenantContext(tenantId, currentRestaurantId));

        db.Restaurants.Add(new Restaurant
        {
            Id = otherRestaurantId,
            TenantId = tenantId,
            Name = "Other Restaurant",
            Domain = $"same-tenant-other-{otherRestaurantId:N}",
            Address = "Other Address",
            City = "Other City",
            State = "Other State",
            IsActive = true
        });
        var foreignLayout = new TableLayout
        {
            TenantId = tenantId,
            RestaurantId = otherRestaurantId,
            Name = $"Tracked Foreign Layout {Guid.NewGuid():N}",
            BackgroundColor = "#ffffff",
            FloorImageUrl = string.Empty
        };
        db.TableLayouts.Add(foreignLayout);
        await db.SaveChangesAsync();

        _ = await db.TableLayouts
            .IgnoreQueryFilters()
            .FirstAsync(x => x.Id == foreignLayout.Id);

        var handler = new UpdateTableLayoutCommandHandler(db, new TestTenantContext(tenantId, currentRestaurantId));

        await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.Handle(new UpdateTableLayoutCommandDto
        {
            Id = foreignLayout.Id,
            Name = "Should Not Update",
            BackgroundColor = "#000000",
            FloorImageUrl = string.Empty
        }, CancellationToken.None));

        Assert.NotEqual("Should Not Update", foreignLayout.Name);
    }

    [Fact]
    public async Task Admin_ShouldManageOwnRestaurantTableLayout()
    {
        var client = await _factory.GetAuthenticatedClientAsync();
        var name = $"Own Layout {Guid.NewGuid():N}";

        var createResponse = await client.PostAsJsonAsync("/api/TableLayout", new
        {
            Name = name,
            BackgroundColor = "#ffffff",
            FloorImageUrl = string.Empty
        });
        createResponse.EnsureSuccessStatusCode();
        var layoutId = await createResponse.Content.ReadFromJsonAsync<int>();

        var list = await client.GetFromJsonAsync<List<TableLayoutDto>>($"/api/TableLayout?name={Uri.EscapeDataString(name)}");
        Assert.Contains(list!, x => x.Id == layoutId);

        var updateResponse = await client.PutAsJsonAsync($"/api/TableLayout/{layoutId}", new
        {
            Name = $"{name} Updated",
            BackgroundColor = "#eeeeee",
            FloorImageUrl = string.Empty
        });
        updateResponse.EnsureSuccessStatusCode();

        var deleteResponse = await client.DeleteAsync($"/api/TableLayout/{layoutId}");
        deleteResponse.EnsureSuccessStatusCode();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        var layout = await db.TableLayouts
            .IgnoreQueryFilters()
            .FirstAsync(x => x.Id == layoutId);

        Assert.True(layout.IsDeleted);
        Assert.Equal(SeedConstants.DefaultTenantId, layout.TenantId);
        Assert.Equal(SeedConstants.DefaultRestaurantId, layout.RestaurantId);
    }

    public static IEnumerable<object[]> CustomerForbiddenRequests()
    {
        yield return new object[]
        {
            (Func<HttpClient, Task<HttpResponseMessage>>)(client =>
                client.PostAsJsonAsync("/api/MealCategory", new
                {
                    Name = "Forbidden category",
                    Description = "Customer must not create meal categories"
                }))
        };

        yield return new object[]
        {
            (Func<HttpClient, Task<HttpResponseMessage>>)(client =>
                client.PostAsJsonAsync("/api/DiningTable", new
                {
                    Number = 901,
                    NumberOfSeats = 4,
                    TableType = 0,
                    TableLayoutId = 1,
                    X = 0,
                    Y = 0
                }))
        };

        yield return new object[]
        {
            (Func<HttpClient, Task<HttpResponseMessage>>)(client =>
                client.PostAsJsonAsync("/api/TableLayout", new
                {
                    Name = "Forbidden layout",
                    BackgroundColor = "#ffffff"
                }))
        };

        yield return new object[]
        {
            (Func<HttpClient, Task<HttpResponseMessage>>)(client =>
                client.GetAsync("/api/Analytics/revenue-per-day"))
        };

        yield return new object[]
        {
            (Func<HttpClient, Task<HttpResponseMessage>>)(client =>
            {
                var form = new MultipartFormDataContent();
                form.Add(new ByteArrayContent("not-an-image"u8.ToArray()), "File", "test.txt");
                return client.PostAsync("/api/File/upload", form);
            })
        };

        yield return new object[]
        {
            (Func<HttpClient, Task<HttpResponseMessage>>)(client =>
                client.PatchAsJsonAsync("/api/TableReservation/update-status", new
                {
                    Id = 1,
                    Status = 1
                }))
        };
    }

    private async Task<HttpClient> CreateCustomerClientAsync()
    {
        var email = $"customer-{Guid.NewGuid():N}@example.test";
        const string password = "customer123";
        var anonymousClient = CreateClientForDomain(DemoRestaurantDomain);

        var registerResponse = await PostCustomerRegistrationAsync(anonymousClient, email, password);
        registerResponse.EnsureSuccessStatusCode();

        return await _factory.GetAuthenticatedClientAsync(email, password);
    }

    private HttpClient CreateClientForDomain(string domain)
    {
        return _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri($"http://{domain}")
        });
    }

    private static Task<HttpResponseMessage> PostCustomerRegistrationAsync(
        HttpClient client,
        string email,
        string password = "customer123")
    {
        return client.PostAsJsonAsync("/api/auth/register/customer", new
        {
            Email = email,
            Password = password,
            DisplayName = "Authorization Test Customer",
            CaptchaToken = CaptchaBypassToken
        });
    }

    private async Task<int> CreateDiningTableForOtherTenantAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

        var tenantId = Guid.NewGuid();
        var restaurantId = Guid.NewGuid();
        db.Tenants.Add(new Tenant
        {
            Id = tenantId,
            Name = "Other Tenant",
            Domain = $"other-{tenantId:N}",
            IsActive = true
        });
        db.Restaurants.Add(new Restaurant
        {
            Id = restaurantId,
            TenantId = tenantId,
            Name = "Other Restaurant",
            Domain = $"other-restaurant-{restaurantId:N}",
            Address = "Other Address",
            City = "Other City",
            State = "Other State",
            IsActive = true
        });

        var layout = new TableLayout
        {
            TenantId = tenantId,
            RestaurantId = restaurantId,
            Name = $"Other Layout {Guid.NewGuid():N}",
            BackgroundColor = "#ffffff",
            FloorImageUrl = string.Empty
        };
        db.TableLayouts.Add(layout);
        await db.SaveChangesAsync();

        var table = new DiningTable
        {
            TenantId = tenantId,
            Number = Random.Shared.Next(10_000, 99_999),
            NumberOfSeats = 4,
            IsActive = true,
            TableLayoutId = layout.Id,
            X = 0,
            Y = 0,
            Width = 100,
            Height = 100
        };
        db.DiningTables.Add(table);
        await db.SaveChangesAsync();

        return table.Id;
    }

    private async Task<int> CreateTableLayoutForOtherTenantAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

        var tenantId = Guid.NewGuid();
        var restaurantId = Guid.NewGuid();
        db.Tenants.Add(new Tenant
        {
            Id = tenantId,
            Name = "Other Tenant",
            Domain = $"other-{tenantId:N}",
            IsActive = true
        });
        db.Restaurants.Add(new Restaurant
        {
            Id = restaurantId,
            TenantId = tenantId,
            Name = "Other Restaurant",
            Domain = $"other-restaurant-{restaurantId:N}",
            Address = "Other Address",
            City = "Other City",
            State = "Other State",
            IsActive = true
        });

        var layout = new TableLayout
        {
            TenantId = tenantId,
            RestaurantId = restaurantId,
            Name = $"Other Layout {Guid.NewGuid():N}",
            BackgroundColor = "#ffffff",
            FloorImageUrl = string.Empty
        };
        db.TableLayouts.Add(layout);
        await db.SaveChangesAsync();

        return layout.Id;
    }

    private async Task<int> CreateTableLayoutForSameTenantOtherRestaurantAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

        var restaurantId = Guid.NewGuid();
        db.Restaurants.Add(new Restaurant
        {
            Id = restaurantId,
            TenantId = SeedConstants.DefaultTenantId,
            Name = "Same Tenant Other Restaurant",
            Domain = $"same-tenant-other-restaurant-{restaurantId:N}",
            Address = "Other Address",
            City = "Other City",
            State = "Other State",
            IsActive = true
        });

        var layout = new TableLayout
        {
            TenantId = SeedConstants.DefaultTenantId,
            RestaurantId = restaurantId,
            Name = $"Same Tenant Other Layout {Guid.NewGuid():N}",
            BackgroundColor = "#ffffff",
            FloorImageUrl = string.Empty
        };
        db.TableLayouts.Add(layout);
        await db.SaveChangesAsync();

        return layout.Id;
    }

    private sealed class TestTenantContext(Guid tenantId, Guid restaurantId) : ITenantContext
    {
        public Guid? TenantId => tenantId;
        public Guid? RestaurantId => restaurantId;
        public bool IsSuperAdmin => false;
    }

    private static string BuildUnsignedJwt(string? tenantClaim)
    {
        var header = new Dictionary<string, object?>
        {
            ["alg"] = "none",
            ["typ"] = "JWT"
        };
        var payload = new Dictionary<string, object?>
        {
            ["sub"] = Guid.NewGuid().ToString(),
            ["aud"] = "biteflow.api",
            ["exp"] = DateTimeOffset.UtcNow.AddMinutes(10).ToUnixTimeSeconds(),
            ["role"] = RoleNames.Staff
        };

        if (tenantClaim is not null)
        {
            payload["tenant_id"] = tenantClaim;
        }

        return $"{Base64Url(header)}.{Base64Url(payload)}.";
    }

    private static string Base64Url(object value)
    {
        var json = JsonSerializer.Serialize(value);
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(json))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
