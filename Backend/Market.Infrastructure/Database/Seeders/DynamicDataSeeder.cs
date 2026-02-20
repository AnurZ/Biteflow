using Market.Domain.Common.Enums;
using Market.Domain.Entities.Catalog;
using Market.Domain.Entities.DiningTables;
using Market.Domain.Entities.Identity;
using Market.Domain.Entities.Meal;
using Market.Domain.Entities.MealCategory;
using Market.Domain.Entities.Orders;
using Market.Domain.Entities.Staff;
using Market.Domain.Entities.TableLayout;
using Market.Domain.Entities.Tenants;
using Market.Shared.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Market.Infrastructure.Database.Seeders;

/// <summary>
/// Dynamic seeder koji se pokrece u runtime-u,
/// obicno pri startu aplikacije (npr. u Program.cs).
/// Koristi se za unos demo/test podataka koji nisu dio migracije.
/// </summary>
public static class DynamicDataSeeder
{
    public static async Task SeedAsync(DatabaseContext context)
    {
        await context.Database.EnsureCreatedAsync();

        await SeedProductCategoriesAsync(context);
        await SeedUsersAsync(context);
        await SeedUserProfilesAsync(context);
        await SeedTenantActivationRequestAsync(context);
        await SeedTableLayoutsAndTablesAsync(context);
        await SeedMealCategoriesAsync(context);
        await SeedMealsAsync(context);
        await SeedOrdersAsync(context);
    }

    private static async Task SeedUserProfilesAsync(DatabaseContext context)
    {
        var entityType = context.Model.FindEntityType(typeof(EmployeeProfile));
        if (entityType?.FindProperty(nameof(EmployeeProfile.AppUserId)) is null)
        {
            Console.WriteLine("Seed skipped: EmployeeProfile.AppUserId not mapped in current context.");
            return;
        }

        await EnsureEmployeeProfileAsync(context, "string", "Manager", "Admin", "User");
        await EnsureEmployeeProfileAsync(context, "waiter1", "Waiter", "Waiter", "One");
        await EnsureEmployeeProfileAsync(context, "kitchen1", "Kitchen", "Kitchen", "One");
    }

    private static async Task SeedProductCategoriesAsync(DatabaseContext context)
    {
        if (!await context.ProductCategories.AnyAsync())
        {
            context.ProductCategories.AddRange(
                new ProductCategoryEntity
                {
                    Name = "Racunari (demo)",
                    IsEnabled = true,
                    CreatedAtUtc = DateTime.UtcNow
                },
                new ProductCategoryEntity
                {
                    Name = "Mobilni uredaji (demo)",
                    IsEnabled = true,
                    CreatedAtUtc = DateTime.UtcNow
                }
            );

            await context.SaveChangesAsync();
            Console.WriteLine("Dynamic seed: product categories added.");
        }
    }

    private static async Task SeedUsersAsync(DatabaseContext context)
    {
        var hasher = new PasswordHasher<AppUser>();

        await EnsureLegacyUserAsync(context, hasher, "admin@market.local", "Admin123!", "ADMIN1");
        await EnsureLegacyUserAsync(context, hasher, "manager@market.local", "User123!", "MANAGER1");
        await EnsureLegacyUserAsync(context, hasher, "string", "string", "Admin1");
        await EnsureLegacyUserAsync(context, hasher, "waiter1", "waiter1", "Waiter1");
        await EnsureLegacyUserAsync(context, hasher, "kitchen1", "kitchen1", "Kitchen1");
        await EnsureLegacyUserAsync(context, hasher, "test", "test123", "TEST1");

        Console.WriteLine("Dynamic seed: demo users added.");
    }

    private static async Task EnsureLegacyUserAsync(
        DatabaseContext context,
        PasswordHasher<AppUser> hasher,
        string email,
        string plainPassword,
        string displayName)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var existing = await context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Email.ToLower() == normalizedEmail);

        if (existing == null)
        {
            var created = new AppUser
            {
                Email = email,
                DisplayName = displayName,
                PasswordHash = hasher.HashPassword(null!, plainPassword),
                IsEnabled = true
            };

            context.Users.Add(created);
            await context.SaveChangesAsync();
            return;
        }

        var changed = false;

        if (!string.Equals(existing.DisplayName, displayName, StringComparison.Ordinal))
        {
            existing.DisplayName = displayName;
            changed = true;
        }

        if (!existing.IsEnabled)
        {
            existing.IsEnabled = true;
            changed = true;
        }

        var passwordCheck = hasher.VerifyHashedPassword(existing, existing.PasswordHash, plainPassword);
        if (passwordCheck == PasswordVerificationResult.Failed)
        {
            existing.PasswordHash = hasher.HashPassword(existing, plainPassword);
            changed = true;
        }

        if (changed)
        {
            await context.SaveChangesAsync();
        }
    }

    private static async Task EnsureEmployeeProfileAsync(
        DatabaseContext context,
        string email,
        string position,
        string firstName,
        string lastName)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var user = await context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Email.ToLower() == normalizedEmail);

        if (user == null)
        {
            Console.WriteLine($"Seed skipped: no user found for '{email}'.");
            return;
        }

        var profile = await context.EmployeeProfiles
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(ep => ep.AppUserId == user.Id);

        if (profile == null)
        {
            profile = new EmployeeProfile
            {
                AppUserId = user.Id,
                TenantId = user.TenantId,
                Position = position,
                FirstName = firstName,
                LastName = lastName,
                IsActive = true
            };

            context.EmployeeProfiles.Add(profile);
            await context.SaveChangesAsync();
            return;
        }

        var changed = false;
        if (!string.Equals(profile.Position, position, StringComparison.Ordinal))
        {
            profile.Position = position;
            changed = true;
        }

        if (!string.Equals(profile.FirstName, firstName, StringComparison.Ordinal))
        {
            profile.FirstName = firstName;
            changed = true;
        }

        if (!string.Equals(profile.LastName, lastName, StringComparison.Ordinal))
        {
            profile.LastName = lastName;
            changed = true;
        }

        if (!profile.IsActive)
        {
            profile.IsActive = true;
            changed = true;
        }

        if (profile.TenantId != user.TenantId)
        {
            profile.TenantId = user.TenantId;
            changed = true;
        }

        if (changed)
        {
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedTenantActivationRequestAsync(DatabaseContext context)
    {
        const string seedDomain = "demo-bistro";

        var alreadyExists = await context.TenantActivationRequests
            .IgnoreQueryFilters()
            .AnyAsync(x => x.Domain == seedDomain);

        if (alreadyExists)
        {
            Console.WriteLine($"Seed skipped: TenantActivationRequest already exists for domain '{seedDomain}'.");
            return;
        }

        var req = new TenantActivationRequest();
        req.EditDraft(
            restaurantName: "Demo Bistro",
            domain: seedDomain,
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

        Console.WriteLine($"Seed: TenantActivationRequest created with Id = {req.Id}");
    }

    private static async Task SeedTableLayoutsAndTablesAsync(DatabaseContext context)
    {
        if (await context.TableLayouts.AnyAsync() || await context.DiningTables.AnyAsync())
        {
            return;
        }

        var layout = new TableLayout
        {
            Name = "Main Floor",
            BackgroundColor = "#f5f5f5",
            FloorImageUrl = string.Empty
        };

        var tables = new List<DiningTable>
        {
            new() { Number = 1, NumberOfSeats = 2, TableLayout = layout, X = 50, Y = 80, Width = 150, Height = 80, Shape = "rectangle", Color = "#e5e7eb", TableType = TableTypes.LowTable, Status = TableStatus.Free, TenantId = SeedConstants.DefaultTenantId },
            new() { Number = 2, NumberOfSeats = 2, TableLayout = layout, X = 220, Y = 80, Width = 150, Height = 80, Shape = "rectangle", Color = "#dbeafe", TableType = TableTypes.LowTable, Status = TableStatus.Free, TenantId = SeedConstants.DefaultTenantId },
            new() { Number = 3, NumberOfSeats = 4, TableLayout = layout, X = 390, Y = 80, Width = 150, Height = 80, Shape = "rectangle", Color = "#dbeafe", TableType = TableTypes.LowTable, Status = TableStatus.Free, TenantId = SeedConstants.DefaultTenantId },
            new() { Number = 4, NumberOfSeats = 4, TableLayout = layout, X = 560, Y = 80, Width = 150, Height = 80, Shape = "rectangle", Color = "#dcfce7", TableType = TableTypes.LowTable, Status = TableStatus.Serving, TenantId = SeedConstants.DefaultTenantId },
            new() { Number = 5, NumberOfSeats = 4, TableLayout = layout, X = 730, Y = 80, Width = 150, Height = 80, Shape = "rectangle", Color = "#e5e7eb", TableType = TableTypes.LowTable, Status = TableStatus.Free, TenantId = SeedConstants.DefaultTenantId },
            new() { Number = 6, NumberOfSeats = 2, TableLayout = layout, X = 900, Y = 80, Width = 150, Height = 80, Shape = "rectangle", Color = "#fee2e2", TableType = TableTypes.LowTable, Status = TableStatus.Paying, TenantId = SeedConstants.DefaultTenantId },
            new() { Number = 9, NumberOfSeats = 6, TableLayout = layout, X = 50, Y = 200, Width = 250, Height = 100, Shape = "rectangle", Color = "#dcfce7", TableType = TableTypes.Hightable, Status = TableStatus.Serving, TenantId = SeedConstants.DefaultTenantId },
            new() { Number = 10, NumberOfSeats = 6, TableLayout = layout, X = 320, Y = 200, Width = 250, Height = 100, Shape = "rectangle", Color = "#dbeafe", TableType = TableTypes.Hightable, Status = TableStatus.Seated, TenantId = SeedConstants.DefaultTenantId },
            new() { Number = 11, NumberOfSeats = 6, TableLayout = layout, X = 590, Y = 200, Width = 250, Height = 100, Shape = "rectangle", Color = "#e5e7eb", TableType = TableTypes.Hightable, Status = TableStatus.Free, TenantId = SeedConstants.DefaultTenantId },
            new() { Number = 12, NumberOfSeats = 6, TableLayout = layout, X = 860, Y = 200, Width = 250, Height = 100, Shape = "rectangle", Color = "#e5e7eb", TableType = TableTypes.Hightable, Status = TableStatus.Free, TenantId = SeedConstants.DefaultTenantId }
        };

        context.TableLayouts.Add(layout);
        context.DiningTables.AddRange(tables);
        await context.SaveChangesAsync();
        Console.WriteLine("Seed: table layout and dining tables added.");
    }

    private static async Task SeedMealCategoriesAsync(DatabaseContext context)
    {
        if (await context.MealCategories.AnyAsync())
        {
            return;
        }

        var now = DateTime.UtcNow;
        var categories = new[]
        {
            new MealCategory { Name = "Starters", Description = "Small bites", TenantId = SeedConstants.DefaultTenantId, CreatedAtUtc = now },
            new MealCategory { Name = "Mains", Description = "Hearty dishes", TenantId = SeedConstants.DefaultTenantId, CreatedAtUtc = now },
            new MealCategory { Name = "Desserts", Description = "Sweet picks", TenantId = SeedConstants.DefaultTenantId, CreatedAtUtc = now },
            new MealCategory { Name = "Drinks", Description = "Beverages", TenantId = SeedConstants.DefaultTenantId, CreatedAtUtc = now }
        };

        context.MealCategories.AddRange(categories);
        await context.SaveChangesAsync();
        Console.WriteLine("Seed: meal categories added.");
    }

    private static async Task SeedMealsAsync(DatabaseContext context)
    {
        if (await context.Meals.AnyAsync())
        {
            return;
        }

        var categories = await context.MealCategories.AsNoTracking().ToListAsync();
        if (categories.Count == 0)
        {
            await SeedMealCategoriesAsync(context);
            categories = await context.MealCategories.AsNoTracking().ToListAsync();
        }

        int Cat(string name) => categories.First(c => c.Name == name).Id;
        var now = DateTime.UtcNow;

        var meals = new List<Meal>
        {
            new()
            {
                Name = "House Burger",
                Description = "Signature burger with cheddar",
                BasePrice = 14.99,
                IsAvailable = true,
                IsFeatured = true,
                ImageField = string.Empty,
                CategoryId = Cat("Mains"),
                TenantId = SeedConstants.DefaultTenantId,
                CreatedAtUtc = now
            },
            new()
            {
                Name = "Truffle Pasta",
                Description = "Creamy truffle fettuccine",
                BasePrice = 18.99,
                IsAvailable = true,
                IsFeatured = true,
                ImageField = string.Empty,
                CategoryId = Cat("Mains"),
                TenantId = SeedConstants.DefaultTenantId,
                CreatedAtUtc = now
            },
            new()
            {
                Name = "Grilled Salmon",
                Description = "Seared salmon with lemon butter",
                BasePrice = 24.99,
                IsAvailable = true,
                IsFeatured = false,
                ImageField = string.Empty,
                CategoryId = Cat("Mains"),
                TenantId = SeedConstants.DefaultTenantId,
                CreatedAtUtc = now
            },
            new()
            {
                Name = "Ribeye Steak",
                Description = "Chargrilled ribeye with herb butter",
                BasePrice = 22.50,
                IsAvailable = true,
                IsFeatured = false,
                ImageField = string.Empty,
                CategoryId = Cat("Mains"),
                TenantId = SeedConstants.DefaultTenantId,
                CreatedAtUtc = now
            },
            new()
            {
                Name = "Caesar Salad",
                Description = "Romaine, parmesan, house dressing",
                BasePrice = 11.50,
                IsAvailable = true,
                IsFeatured = false,
                ImageField = string.Empty,
                CategoryId = Cat("Starters"),
                TenantId = SeedConstants.DefaultTenantId,
                CreatedAtUtc = now
            },
            new()
            {
                Name = "Tomato Soup",
                Description = "Slow-roasted tomato soup",
                BasePrice = 8.25,
                IsAvailable = true,
                IsFeatured = false,
                ImageField = string.Empty,
                CategoryId = Cat("Starters"),
                TenantId = SeedConstants.DefaultTenantId,
                CreatedAtUtc = now
            },
            new()
            {
                Name = "Cheesecake",
                Description = "Classic vanilla cheesecake",
                BasePrice = 7.75,
                IsAvailable = true,
                IsFeatured = false,
                ImageField = string.Empty,
                CategoryId = Cat("Desserts"),
                TenantId = SeedConstants.DefaultTenantId,
                CreatedAtUtc = now
            },
            new()
            {
                Name = "Tiramisu",
                Description = "Coffee-soaked ladyfingers & mascarpone",
                BasePrice = 7.95,
                IsAvailable = true,
                IsFeatured = false,
                ImageField = string.Empty,
                CategoryId = Cat("Desserts"),
                TenantId = SeedConstants.DefaultTenantId,
                CreatedAtUtc = now
            },
            new()
            {
                Name = "Espresso",
                Description = "Single shot",
                BasePrice = 3.50,
                IsAvailable = true,
                IsFeatured = false,
                ImageField = string.Empty,
                CategoryId = Cat("Drinks"),
                TenantId = SeedConstants.DefaultTenantId,
                CreatedAtUtc = now
            },
            new()
            {
                Name = "Iced Tea",
                Description = "Refreshing lemon iced tea",
                BasePrice = 4.25,
                IsAvailable = true,
                IsFeatured = false,
                ImageField = string.Empty,
                CategoryId = Cat("Drinks"),
                TenantId = SeedConstants.DefaultTenantId,
                CreatedAtUtc = now
            }
        };

        context.Meals.AddRange(meals);
        await context.SaveChangesAsync();
        Console.WriteLine("Seed: meals added.");
    }

    private static async Task SeedOrdersAsync(DatabaseContext context)
    {
        if (await context.Orders.AnyAsync())
        {
            return;
        }

        var categories = await context.MealCategories.AsNoTracking().ToListAsync();
        if (categories.Count == 0)
        {
            await SeedMealCategoriesAsync(context);
            categories = await context.MealCategories.AsNoTracking().ToListAsync();
        }

        var meals = await context.Meals.ToListAsync();
        var newMeals = new List<Meal>();

        Meal EnsureMeal(string name, double price, string categoryName)
        {
            var existing = meals.FirstOrDefault(m => m.Name == name);
            if (existing != null) return existing;

            var categoryId = categories.FirstOrDefault(c => c.Name == categoryName)?.Id
                             ?? categories.First().Id;

            var meal = new Meal
            {
                Name = name,
                Description = $"{name} (seed)",
                BasePrice = price,
                IsAvailable = true,
                IsFeatured = false,
                ImageField = string.Empty,
                CategoryId = categoryId,
                TenantId = SeedConstants.DefaultTenantId,
                CreatedAtUtc = DateTime.UtcNow
            };

            meals.Add(meal);
            newMeals.Add(meal);
            return meal;
        }

        var mBurger = EnsureMeal("House Burger", 14.99, "Mains");
        var mCaesar = EnsureMeal("Caesar Salad", 11.50, "Starters");
        var mSalmon = EnsureMeal("Grilled Salmon", 24.99, "Mains");
        var mTruffle = EnsureMeal("Truffle Pasta", 18.99, "Mains");
        var mRibeye = EnsureMeal("Ribeye Steak", 22.50, "Mains");
        var mTiramisu = EnsureMeal("Tiramisu", 7.95, "Desserts");

        if (newMeals.Count > 0)
        {
            context.Meals.AddRange(newMeals);
            await context.SaveChangesAsync();

            meals = await context.Meals.AsNoTracking().ToListAsync();
            mBurger = meals.First(m => m.Name == "House Burger");
            mCaesar = meals.First(m => m.Name == "Caesar Salad");
            mSalmon = meals.First(m => m.Name == "Grilled Salmon");
            mTruffle = meals.First(m => m.Name == "Truffle Pasta");
            mRibeye = meals.First(m => m.Name == "Ribeye Steak");
            mTiramisu = meals.First(m => m.Name == "Tiramisu");
        }

        var order1 = new Order
        {
            TableNumber = 3,
            Status = OrderStatus.New,
            TenantId = SeedConstants.DefaultTenantId,
            Items = new List<OrderItem>
            {
                new() { Name = "House Burger", Quantity = 2, UnitPrice = 14.99m, MealId = mBurger.Id, TenantId = SeedConstants.DefaultTenantId },
                new() { Name = "Caesar Salad", Quantity = 2, UnitPrice = 11.50m, MealId = mCaesar.Id, TenantId = SeedConstants.DefaultTenantId }
            }
        };

        var order2 = new Order
        {
            TableNumber = 4,
            Status = OrderStatus.Cooking,
            TenantId = SeedConstants.DefaultTenantId,
            Items = new List<OrderItem>
            {
                new() { Name = "Grilled Salmon", Quantity = 1, UnitPrice = 24.99m, MealId = mSalmon.Id, TenantId = SeedConstants.DefaultTenantId },
                new() { Name = "Truffle Pasta", Quantity = 2, UnitPrice = 18.99m, MealId = mTruffle.Id, TenantId = SeedConstants.DefaultTenantId }
            }
        };

        var order3 = new Order
        {
            TableNumber = 6,
            Status = OrderStatus.ReadyForPickup,
            TenantId = SeedConstants.DefaultTenantId,
            Items = new List<OrderItem>
            {
                new() { Name = "Ribeye Steak", Quantity = 2, UnitPrice = 22.50m, MealId = mRibeye.Id, TenantId = SeedConstants.DefaultTenantId },
                new() { Name = "Tiramisu", Quantity = 2, UnitPrice = 7.95m, MealId = mTiramisu.Id, TenantId = SeedConstants.DefaultTenantId }
            }
        };

        context.Orders.AddRange(order1, order2, order3);
        await context.SaveChangesAsync();
        Console.WriteLine("Seed: orders with items added.");
    }
}
