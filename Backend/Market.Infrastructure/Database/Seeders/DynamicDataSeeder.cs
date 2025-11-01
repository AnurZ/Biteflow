using Market.Domain.Entities.Staff;
using Market.Domain.Entities.Tenants;

namespace Market.Infrastructure.Database.Seeders;

/// <summary>
/// Dynamic seeder koji se pokreće u runtime-u,
/// obično pri startu aplikacije (npr. u Program.cs).
/// Koristi se za unos demo/test podataka koji nisu dio migracije.
/// </summary>
public static class DynamicDataSeeder
{
    public static async Task SeedAsync(DatabaseContext context)
    {
        // Osiguraj da baza postoji (bez migracija)
        await context.Database.EnsureCreatedAsync();

        await SeedProductCategoriesAsync(context);
        await SeedUsersAsync(context);
        await SeedUserProfilesAsync(context);
        await SeedTenantActivationRequestAsync(context);
    }

    private static async Task SeedUserProfilesAsync(DatabaseContext context)
    {
        var user = await context.Users
            .Where(u => EF.Functions.Like(u.Email, "%string%"))
            .FirstOrDefaultAsync();

        if (user == null)
        {
            Console.WriteLine("ℹ️ Seed skipped: no user found whose email contains 'string'.");
            return;
        }

        // 2) Avoid duplicates (if multi-tenant, also filter by TenantId)
        var hasAny = await context.EmployeeProfiles
            .AnyAsync(ep => ep.AppUserId == user.Id);
        if (hasAny)
        {
            Console.WriteLine("ℹ️ Seed skipped: EmployeeProfile already exists for this user.");
            return;
        }

        // 3) Create profile (only set FK; no need to set navigation explicitly)
        var profile = new EmployeeProfile
        {
            AppUserId = user.Id,
            Position = "Waiter",
            FirstName = "Anur",
            LastName = "Zjakic",
            PhoneNumber = "123123123",
            HireDate = DateTime.UtcNow,
            Salary = 1200m,
            HourlyRate = 5m,
            EmploymentType = "FullTime",
            ShiftType = "Morning",
            ShiftStart = TimeOnly.FromTimeSpan(TimeSpan.FromHours(8)),
            ShiftEnd = TimeOnly.FromTimeSpan(TimeSpan.FromHours(15)),
            AverageRating = 4.39,
            CompletedOrders = 10,
            MonthlyTips = 11m,
            IsActive = true
        };

        context.EmployeeProfiles.Add(profile);
        await context.SaveChangesAsync();
        Console.WriteLine("✅ Seed: EmployeeProfile added.");
    }


    private static async Task SeedProductCategoriesAsync(DatabaseContext context)
    {
        if (!await context.ProductCategories.AnyAsync())
        {
            context.ProductCategories.AddRange(
                new ProductCategoryEntity
                {
                    Name = "Računari (demo)",
                    IsEnabled = true,
                    CreatedAtUtc = DateTime.UtcNow
                },
                new ProductCategoryEntity
                {
                    Name = "Mobilni uređaji (demo)",
                    IsEnabled = true,
                    CreatedAtUtc = DateTime.UtcNow
                }
            );

            await context.SaveChangesAsync();
            Console.WriteLine("✅ Dynamic seed: product categories added.");
        }
    }

    /// <summary>
    /// Kreira demo korisnike ako ih još nema u bazi.
    /// </summary>
    private static async Task SeedUsersAsync(DatabaseContext context)
    {
        if (await context.Users.AnyAsync())
            return;

        var hasher = new PasswordHasher<MarketUserEntity>();

        var admin = new AppUser
        {
            Email = "admin@market.local",
            PasswordHash = hasher.HashPassword(null!, "Admin123!"),
            IsEnabled = true,
            DisplayName = "ADMIN1",
        };

        var user = new AppUser
        {
            Email = "manager@market.local",
            PasswordHash = hasher.HashPassword(null!, "User123!"),
            IsEnabled = true,
            DisplayName = "MANAGER1"
        };

        var dummyForSwagger = new AppUser
        {
            Email = "string",
            PasswordHash = hasher.HashPassword(null!, "string"),
            IsEnabled = true,
            DisplayName = "Konobar1"
        };
        var dummyForTests = new AppUser
        {
            Email = "test",
            PasswordHash = hasher.HashPassword(null!, "test123"),
            IsEnabled = true,
            DisplayName = "TEST1"
        };
        context.Users.AddRange(admin, user, dummyForSwagger, dummyForTests);
        await context.SaveChangesAsync();

        Console.WriteLine("✅ Dynamic seed: demo users added.");
    }

    private static async Task SeedTenantActivationRequestAsync(DatabaseContext context)
    {
        if (await context.TenantActivationRequests.AnyAsync()) return;

        var req = new TenantActivationRequest();
        req.EditDraft(
            restaurantName: "Demo Bistro",
            domain: "demo-bistro",
            ownerFullName: "Owner Name",
            ownerEmail: "owner@example.com",
            ownerPhone: "+38761111222",
            address: "Ulica 1",
            city: "Mostar",
            state: "FBIH"
        );
        req.Submit(); // status -> Submitted

        context.TenantActivationRequests.Add(req);
        await context.SaveChangesAsync();

        Console.WriteLine($"✅ Seed: TenantActivationRequest created with Id = {req.Id}");
    }

}