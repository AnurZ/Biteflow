using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Stores;
using Market.Application.Abstractions;
using Market.Application.Modules.TableLayout.Commands.UpdateTableLayout;
using Market.Application.Modules.TableLayout.Querries.GetTableLayouts;
using Market.Domain.Common.Enums;
using Market.Domain.Entities.Catalog;
using Market.Domain.Entities.DiningTables;
using Market.Domain.Entities.InventoryItem;
using Market.Domain.Entities.Meal;
using Market.Domain.Entities.MealCategory;
using Market.Domain.Entities.TableLayout;
using Market.Domain.Entities.TableReservations;
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
    [InlineData("waiter1", "WaiterUser1!", "/api/TableLayout")]
    [InlineData("string", "StringUser1!", "/api/Analytics/revenue-per-day")]
    [InlineData("string", "StringUser1!", "/api/inventoryitem")]
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

    [Theory]
    [InlineData(RoleNames.SuperAdmin)]
    public async Task RestaurantAdmin_ShouldNotCreatePrivilegedStaffRole(string role)
    {
        var email = $"privilege-escalation-{role}-{Guid.NewGuid():N}@example.test";
        var client = await _factory.GetAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/api/Staff", CreateStaffPayload(email, role));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.FindByEmailAsync(email);

        Assert.Null(user);
    }

    [Theory]
    [InlineData(RoleNames.Admin)]
    [InlineData(RoleNames.Waiter)]
    [InlineData(RoleNames.Kitchen)]
    public async Task RestaurantAdmin_ShouldCreateOnlyRestaurantScopedStaffRoles(string role)
    {
        var email = $"restaurant-staff-{role}-{Guid.NewGuid():N}@example.test";
        var client = await _factory.GetAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/api/Staff", CreateStaffPayload(email, role));

        response.EnsureSuccessStatusCode();

        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        var user = await userManager.FindByEmailAsync(email);

        Assert.NotNull(user);
        Assert.Equal(SeedConstants.DefaultTenantId, user!.TenantId);
        Assert.Equal(SeedConstants.DefaultRestaurantId, user.RestaurantId);
        Assert.True(await userManager.IsInRoleAsync(user, role));

        var profile = await db.EmployeeProfiles
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.ApplicationUserId == user.Id);

        Assert.NotNull(profile);
        Assert.Equal(SeedConstants.DefaultTenantId, profile!.TenantId);
        Assert.Equal(ExpectedPositionForRole(role), profile.Position);
    }

    [Fact]
    public async Task RestaurantAdmin_ShouldUpdateRoleAndDerivedPosition()
    {
        var email = $"restaurant-staff-update-role-{Guid.NewGuid():N}@example.test";
        var client = await _factory.GetAuthenticatedClientAsync();

        var createResponse = await client.PostAsJsonAsync("/api/Staff", CreateStaffPayload(email, RoleNames.Waiter));
        createResponse.EnsureSuccessStatusCode();
        var staffId = await ReadCreatedIdAsync(createResponse);

        var updateResponse = await client.PutAsJsonAsync($"/api/Staff/{staffId}", new
        {
            DisplayName = "Restaurant Staff Test",
            Role = RoleNames.Kitchen,
            FirstName = "Restaurant",
            LastName = "Staff",
            PhoneNumber = "123456",
            HireDate = DateTime.UtcNow.Date,
            HourlyRate = 10m,
            EmploymentType = "FullTime",
            ShiftType = "Morning",
            ShiftStart = "08:00:00",
            ShiftEnd = "16:00:00",
            IsActive = true,
            Notes = "Authorization integration test"
        });

        updateResponse.EnsureSuccessStatusCode();

        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        var user = await userManager.FindByEmailAsync(email);

        Assert.NotNull(user);
        Assert.True(await userManager.IsInRoleAsync(user!, RoleNames.Kitchen));
        Assert.False(await userManager.IsInRoleAsync(user!, RoleNames.Waiter));

        var profile = await db.EmployeeProfiles
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.ApplicationUserId == user.Id);

        Assert.NotNull(profile);
        Assert.Equal("Cook", profile!.Position);
    }

    [Fact]
    public async Task RestaurantAdmin_ShouldSecurelyDeleteStaffIdentityAndProfile()
    {
        var email = $"delete-staff-{Guid.NewGuid():N}@example.test";
        const string password = "StaffPass123!";
        var adminClient = await _factory.GetAuthenticatedClientAsync();

        var createResponse = await adminClient.PostAsJsonAsync("/api/Staff", CreateStaffPayload(email, RoleNames.Waiter));
        createResponse.EnsureSuccessStatusCode();
        var staffId = await ReadCreatedIdAsync(createResponse);

        Guid userId;
        using (var setupScope = _factory.Services.CreateScope())
        {
            var userManager = setupScope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var persistedGrantStore = setupScope.ServiceProvider.GetRequiredService<IPersistedGrantStore>();
            var user = await userManager.FindByEmailAsync(email);

            Assert.NotNull(user);
            userId = user!.Id;

            await userManager.AddToRolesAsync(user, new[] { RoleNames.Admin, RoleNames.Customer, RoleNames.SuperAdmin });

            await persistedGrantStore.StoreAsync(new PersistedGrant
            {
                Key = $"refresh-{Guid.NewGuid():N}",
                Type = "refresh_token",
                SubjectId = user.Id.ToString(),
                ClientId = "biteflow-angular",
                CreationTime = DateTime.UtcNow,
                Expiration = DateTime.UtcNow.AddDays(30),
                Data = "{}"
            });

            await persistedGrantStore.StoreAsync(new PersistedGrant
            {
                Key = $"consent-{Guid.NewGuid():N}",
                Type = "user_consent",
                SubjectId = user.Id.ToString(),
                ClientId = "biteflow-angular",
                CreationTime = DateTime.UtcNow,
                Expiration = DateTime.UtcNow.AddDays(30),
                Data = "{}"
            });
        }

        var deleteResponse = await adminClient.DeleteAsync($"/api/Staff/{staffId}");

        deleteResponse.EnsureSuccessStatusCode();

        using (var assertScope = _factory.Services.CreateScope())
        {
            var userManager = assertScope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var db = assertScope.ServiceProvider.GetRequiredService<DatabaseContext>();
            var persistedGrantStore = assertScope.ServiceProvider.GetRequiredService<IPersistedGrantStore>();
            var user = await userManager.FindByIdAsync(userId.ToString());

            Assert.NotNull(user);
            Assert.False(user!.IsEnabled);
            Assert.True(user.LockoutEnabled);
            Assert.NotNull(user.LockoutEnd);
            Assert.True(user.LockoutEnd > DateTimeOffset.UtcNow.AddYears(100));

            Assert.False(await userManager.IsInRoleAsync(user, RoleNames.Admin));
            Assert.False(await userManager.IsInRoleAsync(user, RoleNames.Staff));
            Assert.False(await userManager.IsInRoleAsync(user, RoleNames.Waiter));
            Assert.False(await userManager.IsInRoleAsync(user, RoleNames.Kitchen));
            Assert.True(await userManager.IsInRoleAsync(user, RoleNames.Customer));
            Assert.True(await userManager.IsInRoleAsync(user, RoleNames.SuperAdmin));

            var profile = await db.EmployeeProfiles
                .IgnoreQueryFilters()
                .SingleAsync(x => x.Id == staffId);
            Assert.True(profile.IsDeleted);

            var refreshGrants = await persistedGrantStore.GetAllAsync(new PersistedGrantFilter
            {
                SubjectId = user.Id.ToString(),
                Type = "refresh_token"
            });
            Assert.Empty(refreshGrants);

            var consentGrants = await persistedGrantStore.GetAllAsync(new PersistedGrantFilter
            {
                SubjectId = user.Id.ToString(),
                Type = "user_consent"
            });
            Assert.NotEmpty(consentGrants);
        }

        var getDeletedResponse = await adminClient.GetAsync($"/api/Staff/{staffId}");
        Assert.False(getDeletedResponse.IsSuccessStatusCode);

        var secondDeleteResponse = await adminClient.DeleteAsync($"/api/Staff/{staffId}");
        secondDeleteResponse.EnsureSuccessStatusCode();

        var loginResponse = await RequestPasswordTokenAsync(_factory.CreateClient(), email, password);
        Assert.Equal(HttpStatusCode.BadRequest, loginResponse.StatusCode);
    }

    [Fact]
    public async Task RestaurantAdmin_ShouldTreatDeleteStaffAsIdempotent()
    {
        var client = await _factory.GetAuthenticatedClientAsync();

        var missingResponse = await client.DeleteAsync($"/api/Staff/{int.MaxValue}");

        missingResponse.EnsureSuccessStatusCode();
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
    public async Task PublicTenantResolver_WithExplicitKnownIds_ShouldReturnTenantContext()
    {
        using var scope = _factory.Services.CreateScope();
        var resolver = scope.ServiceProvider.GetRequiredService<IPublicTenantResolver>();

        var context = await resolver.ResolveRequiredAsync(
            SeedConstants.DefaultTenantId,
            SeedConstants.DefaultRestaurantId);

        Assert.Equal(SeedConstants.DefaultTenantId, context.TenantId);
        Assert.Equal(SeedConstants.DefaultRestaurantId, context.RestaurantId);
        Assert.Equal(DemoRestaurantDomain, context.Domain);
    }

    [Fact]
    public async Task PublicTenantResolver_WithExplicitMismatchedIds_ShouldRejectContext()
    {
        using var scope = _factory.Services.CreateScope();
        var resolver = scope.ServiceProvider.GetRequiredService<IPublicTenantResolver>();

        await Assert.ThrowsAsync<FluentValidation.ValidationException>(() =>
            resolver.ResolveRequiredAsync(SeedConstants.DefaultTenantId, Guid.NewGuid()));
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
    public async Task Anonymous_ShouldSubmitTenantActivationRequestWithSinglePost()
    {
        var client = _factory.CreateClient();
        var domain = $"join-{Guid.NewGuid():N}";
        var ownerEmail = $"join-{Guid.NewGuid():N}@example.test";

        var createResponse = await client.PostAsJsonAsync("/api/activation-requests", new
        {
            RestaurantName = "Join Test Bistro",
            Domain = domain,
            OwnerFullName = "Join Owner",
            OwnerEmail = ownerEmail,
            OwnerPhone = "123456",
            Address = "Join Address",
            City = "Mostar",
            State = "FBIH"
        });

        createResponse.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.NoContent, createResponse.StatusCode);

        var duplicateResponse = await client.PostAsJsonAsync("/api/activation-requests", new
        {
            RestaurantName = "Join Test Bistro Duplicate",
            Domain = domain,
            OwnerFullName = "Join Owner Duplicate",
            OwnerEmail = $"join-duplicate-{Guid.NewGuid():N}@example.test",
            OwnerPhone = "654321",
            Address = "Duplicate Join Address",
            City = "Sarajevo",
            State = "FBIH"
        });
        duplicateResponse.EnsureSuccessStatusCode();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        var request = await db.TenantActivationRequests
            .IgnoreQueryFilters()
            .FirstAsync(x => x.Domain == domain);
        var requestCount = await db.TenantActivationRequests
            .IgnoreQueryFilters()
            .CountAsync(x => x.Domain == domain);

        var numericGetResponse = await client.GetAsync($"/api/activation-requests/{request.Id}");
        var numericUpdateResponse = await client.PutAsJsonAsync($"/api/activation-requests/{request.Id}", new
        {
            RestaurantName = "Should Not Update",
            Domain = $"{domain}-numeric",
            OwnerFullName = "Join Owner",
            OwnerEmail = $"join-numeric-{Guid.NewGuid():N}@example.test",
            OwnerPhone = "000000",
            Address = "Numeric Address",
            City = "Sarajevo",
            State = "FBIH"
        });
        var numericSubmitResponse = await client.PostAsync($"/api/activation-requests/{request.Id}/submit", null);

        Assert.Equal(Guid.Empty, request.TenantId);
        Assert.Equal(domain, request.Domain);
        Assert.Equal(ownerEmail, request.OwnerEmail);
        Assert.Equal(ActivationStatus.Submitted, request.Status);
        Assert.Equal(1, requestCount);
        Assert.False(numericGetResponse.IsSuccessStatusCode);
        Assert.False(numericUpdateResponse.IsSuccessStatusCode);
        Assert.False(numericSubmitResponse.IsSuccessStatusCode);

        var superAdmin = await _factory.GetAuthenticatedClientAsync("superadmin", "Superadmin1!");
        var adminListResponse = await superAdmin.GetAsync("/api/activation-requests");
        adminListResponse.EnsureSuccessStatusCode();
        using var adminListPayload = JsonDocument.Parse(await adminListResponse.Content.ReadAsStringAsync());
        var adminItem = adminListPayload.RootElement.GetProperty("items").EnumerateArray()
            .First(x => x.GetProperty("id").GetInt32() == request.Id);
        Assert.Equal(domain, adminItem.GetProperty("domain").GetString());
    }

    [Fact]
    public async Task ConfirmActivation_ShouldUseOneTimePasswordSetupLinkWithoutLeakingPassword()
    {
        CustomWebApplicationFactory<Program>.ClearSentEmails();
        var client = _factory.CreateClient();
        var restaurantName = $"Da Vinci {Guid.NewGuid():N}";
        var ownerEmail = $"activation-owner-{Guid.NewGuid():N}@example.test";
        var domain = $"activation-secure-{Guid.NewGuid():N}";

        var createResponse = await client.PostAsJsonAsync("/api/activation-requests", new
        {
            RestaurantName = restaurantName,
            Domain = domain,
            OwnerFullName = "Activation Owner",
            OwnerEmail = ownerEmail,
            OwnerPhone = "123456",
            Address = "Activation Address",
            City = "Mostar",
            State = "FBIH"
        });
        createResponse.EnsureSuccessStatusCode();

        using var requestScope = _factory.Services.CreateScope();
        var requestDb = requestScope.ServiceProvider.GetRequiredService<DatabaseContext>();
        var requestId = await requestDb.TenantActivationRequests
            .IgnoreQueryFilters()
            .Where(x => x.Domain == domain)
            .Select(x => x.Id)
            .SingleAsync();

        var superAdmin = await _factory.GetAuthenticatedClientAsync("superadmin", "Superadmin1!");
        var approveResponse = await superAdmin.PostAsync($"/api/activation-requests/{requestId}/approve", null);
        approveResponse.EnsureSuccessStatusCode();
        var activationLink = await approveResponse.Content.ReadAsStringAsync();
        var activationToken = GetQueryParam(activationLink, "token");

        CustomWebApplicationFactory<Program>.ClearSentEmails();
        var confirmResponse = await client.PostAsJsonAsync("/api/activation-requests/confirm", new
        {
            Token = activationToken
        });
        confirmResponse.EnsureSuccessStatusCode();

        using var payload = JsonDocument.Parse(await confirmResponse.Content.ReadAsStringAsync());
        var root = payload.RootElement;
        Assert.True(root.TryGetProperty("tenantId", out _));
        Assert.True(root.TryGetProperty("adminUsername", out var adminUsernameProperty));
        Assert.False(root.TryGetProperty("adminPassword", out _));
        Assert.False(root.TryGetProperty("restaurantName", out _));

        var adminUsername = adminUsernameProperty.GetString()!;
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var admin = await userManager.FindByEmailAsync(adminUsername);

        Assert.NotNull(admin);
        Assert.True(await userManager.IsInRoleAsync(admin!, RoleNames.Admin));
        Assert.False(await userManager.CheckPasswordAsync(admin!, "davincifirstpassword"));

        var emails = CustomWebApplicationFactory<Program>.GetSentEmails();
        var onboardingEmail = Assert.Single(emails, x => x.ToEmail == ownerEmail);
        Assert.Contains(adminUsername, onboardingEmail.Body);
        Assert.Contains("/activate/set-password", onboardingEmail.Body);
        Assert.DoesNotContain("firstpassword", onboardingEmail.Body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("First-time password", onboardingEmail.Body, StringComparison.OrdinalIgnoreCase);

        var setupLink = onboardingEmail.Body
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Single(x => x.Contains("/activate/set-password", StringComparison.OrdinalIgnoreCase));
        setupLink = setupLink[setupLink.IndexOf("http", StringComparison.OrdinalIgnoreCase)..];
        var setupUserId = GetQueryParam(setupLink, "userId");
        var setupToken = GetQueryParam(setupLink, "token");
        var newPassword = $"Newpassword1!{Guid.NewGuid():N}";

        var setPasswordResponse = await client.PostAsJsonAsync("/api/auth/set-password", new
        {
            UserId = setupUserId,
            Token = setupToken,
            Password = newPassword
        });
        setPasswordResponse.EnsureSuccessStatusCode();

        var reuseResponse = await client.PostAsJsonAsync("/api/auth/set-password", new
        {
            UserId = setupUserId,
            Token = setupToken,
            Password = $"{newPassword}2"
        });
        Assert.Equal(HttpStatusCode.BadRequest, reuseResponse.StatusCode);

        var invalidTokenResponse = await client.PostAsJsonAsync("/api/auth/set-password", new
        {
            UserId = setupUserId,
            Token = "invalid-token",
            Password = $"{newPassword}3"
        });
        Assert.Equal(HttpStatusCode.BadRequest, invalidTokenResponse.StatusCode);

        var login = await _factory.GetAuthenticatedClientAsync(adminUsername, newPassword);
        var protectedResponse = await login.GetAsync("/api/Analytics/revenue-per-day");
        protectedResponse.EnsureSuccessStatusCode();
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
    public async Task Admin_ShouldNotMutateResourcesFromAnotherTenant()
    {
        var ids = await CreateMutableResourcesForOtherTenantAsync();
        var client = await _factory.GetAuthenticatedClientAsync();

        var responses = new[]
        {
            await client.PutAsJsonAsync($"/api/DiningTable/{ids.DiningTableId}", new
            {
                Number = 77701,
                NumberOfSeats = 4,
                IsActive = true,
                ids.TableLayoutId,
                X = 10,
                Y = 10,
                Height = 100,
                Width = 100,
                Shape = "rectangle",
                Color = "#ffffff",
                TableType = 0,
                Status = 0,
                LastUsedAt = (DateTime?)null
            }),
            await client.DeleteAsync($"/api/DiningTable/{ids.DiningTableId}"),
            await client.PutAsJsonAsync($"/api/MealCategory/{ids.MealCategoryId}", new
            {
                Name = "Should Not Update",
                Description = "Foreign"
            }),
            await client.DeleteAsync($"/api/MealCategory/{ids.MealCategoryId}"),
            await client.PutAsJsonAsync($"/api/Meal/{ids.MealId}", new
            {
                Name = "Should Not Update",
                Description = "Foreign",
                BasePrice = 10,
                IsAvailable = true,
                IsFeatured = false,
                ImageField = "",
                StockManaged = false,
                CategoryId = (int?)null,
                Ingredients = Array.Empty<object>()
            }),
            await client.DeleteAsync($"/api/Meal/{ids.MealId}"),
            await client.PutAsJsonAsync($"/api/InventoryItem/{ids.InventoryItemId}", new
            {
                ids.RestaurantId,
                Name = "Should Not Update",
                Sku = "foreign-sku",
                UnitType = 0,
                ReorderQty = 1,
                ReorderFrequency = 1,
                CurrentQty = 1
            }),
            await client.DeleteAsync($"/api/InventoryItem/{ids.InventoryItemId}"),
            await client.PutAsJsonAsync($"/ProductCategories/{ids.ProductCategoryId}", new
            {
                Name = "Should Not Update"
            }),
            await client.PutAsync($"/ProductCategories/{ids.ProductCategoryId}/disable", null),
            await client.DeleteAsync($"/ProductCategories/{ids.ProductCategoryId}"),
            await client.PutAsJsonAsync($"/api/TableReservation/{ids.TableReservationId}", new
            {
                ids.DiningTableId,
                NumberOfGuests = 2,
                ReservationStart = DateTime.UtcNow.AddDays(30),
                ReservationEnd = DateTime.UtcNow.AddDays(30).AddHours(1),
                FirstName = "Foreign",
                LastName = "Guest",
                Email = "foreign@example.test",
                PhoneNumber = "123",
                Status = 0
            }),
            await client.PatchAsJsonAsync("/api/TableReservation/update-status", new
            {
                Id = ids.TableReservationId,
                Status = 1
            }),
            await client.DeleteAsync($"/api/TableReservation/{ids.TableReservationId}")
        };

        foreach (var response in responses)
        {
            var body = await response.Content.ReadAsStringAsync();
            Assert.True(
                response.StatusCode == HttpStatusCode.NotFound,
                $"Expected 404, got {(int)response.StatusCode} {response.StatusCode}. Body: {body}");
        }

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        var meal = await db.Meals.IgnoreQueryFilters().FirstAsync(x => x.Id == ids.MealId);
        var inventoryItem = await db.InventoryItems.IgnoreQueryFilters().FirstAsync(x => x.Id == ids.InventoryItemId);
        var productCategory = await db.ProductCategories.IgnoreQueryFilters().FirstAsync(x => x.Id == ids.ProductCategoryId);
        var reservation = await db.TableReservations.IgnoreQueryFilters().FirstAsync(x => x.Id == ids.TableReservationId);

        Assert.NotEqual("Should Not Update", meal.Name);
        Assert.NotEqual("Should Not Update", inventoryItem.Name);
        Assert.NotEqual("Should Not Update", productCategory.Name);
        Assert.False(productCategory.IsDeleted);
        Assert.Equal(ReservationStatus.Pending, reservation.Status);
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
        //Assert.Equal(SeedConstants.DefaultRestaurantId, layout.RestaurantId);
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
        const string password = "Customer123!";
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
        string password = "Customer123!")
    {
        return client.PostAsJsonAsync("/api/auth/register/customer", new
        {
            Email = email,
            Password = password,
            DisplayName = "Authorization Test Customer",
            CaptchaToken = CaptchaBypassToken
        });
    }

    private static object CreateStaffPayload(string email, string? role)
    {
        return new
        {
            Email = email,
            DisplayName = "Restaurant Staff Test",
            PlainPassword = "StaffPass123!",
            Role = role,
            FirstName = "Restaurant",
            LastName = "Staff",
            PhoneNumber = "123456",
            HireDate = DateTime.UtcNow.Date,
            HourlyRate = 10m,
            EmploymentType = "FullTime",
            ShiftType = "Morning",
            ShiftStart = "08:00:00",
            ShiftEnd = "16:00:00",
            IsActive = true,
            Notes = "Authorization integration test"
        };
    }

    private static string ExpectedPositionForRole(string role)
    {
        return role.ToLowerInvariant() switch
        {
            RoleNames.Admin => "Manager",
            RoleNames.Waiter => "Waiter",
            RoleNames.Kitchen => "Cook",
            _ => "Manager"
        };
    }

    private static async Task<int> ReadCreatedIdAsync(HttpResponseMessage response)
    {
        using var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return payload.RootElement.GetProperty("id").GetInt32();
    }

    private static async Task<HttpResponseMessage> RequestPasswordTokenAsync(
        HttpClient client,
        string username,
        string password)
    {
        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["client_id"] = "biteflow-tests",
            ["username"] = username,
            ["password"] = password,
            ["scope"] = "openid profile email roles biteflow.api"
        });

        return await client.PostAsync("connect/token", content);
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
            Name = $"Same Tenant Other Layout {Guid.NewGuid():N}",
            BackgroundColor = "#ffffff",
            FloorImageUrl = string.Empty
        };
        db.TableLayouts.Add(layout);
        await db.SaveChangesAsync();

        return layout.Id;
    }

    private async Task<CrossTenantResourceIds> CreateMutableResourcesForOtherTenantAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

        var tenantId = Guid.NewGuid();
        var restaurantId = Guid.NewGuid();

        db.Tenants.Add(new Tenant
        {
            Id = tenantId,
            Name = $"Mutable Other Tenant {tenantId:N}",
            Domain = $"mutable-other-{tenantId:N}",
            IsActive = true
        });

        db.Restaurants.Add(new Restaurant
        {
            Id = restaurantId,
            TenantId = tenantId,
            Name = "Mutable Other Restaurant",
            Domain = $"mutable-other-restaurant-{restaurantId:N}",
            Address = "Other Address",
            City = "Other City",
            State = "Other State",
            IsActive = true
        });

        var layout = new TableLayout
        {
            TenantId = tenantId,
            Name = $"Mutable Other Layout {Guid.NewGuid():N}",
            BackgroundColor = "#ffffff",
            FloorImageUrl = string.Empty
        };
        db.TableLayouts.Add(layout);
        await db.SaveChangesAsync();

        var diningTable = new DiningTable
        {
            TenantId = tenantId,
            Number = Random.Shared.Next(10_000, 99_999),
            NumberOfSeats = 4,
            IsActive = true,
            TableLayoutId = layout.Id,
            X = 0,
            Y = 0,
            Width = 100,
            Height = 100,
            Shape = "rectangle",
            Color = "#ffffff"
        };
        var reservationTable = new DiningTable
        {
            TenantId = tenantId,
            Number = Random.Shared.Next(10_000, 99_999),
            NumberOfSeats = 4,
            IsActive = true,
            TableLayoutId = layout.Id,
            X = 0,
            Y = 120,
            Width = 100,
            Height = 100,
            Shape = "rectangle",
            Color = "#ffffff"
        };
        var mealCategory = new MealCategory
        {
            TenantId = tenantId,
            RestaurantId = restaurantId,
            Name = $"Mutable Other Meal Category {Guid.NewGuid():N}",
            Description = "Other"
        };
        var meal = new Meal
        {
            TenantId = tenantId,
            RestaurantId = restaurantId,
            Name = $"Mutable Other Meal {Guid.NewGuid():N}",
            Description = "Other",
            BasePrice = 10,
            IsAvailable = true,
            ImageField = string.Empty
        };
        var inventoryItem = new InventoryItem
        {
            TenantId = tenantId,
            RestaurantId = restaurantId,
            Name = $"Mutable Other Inventory {Guid.NewGuid():N}",
            Sku = $"sku-{Guid.NewGuid():N}",
            UnitType = 0,
            ReorderQty = 1,
            ReorderFrequency = 1,
            CurrentQty = 1
        };
        var productCategory = new ProductCategoryEntity
        {
            TenantId = tenantId,
            Name = $"Mutable Other Product Category {Guid.NewGuid():N}",
            IsEnabled = true
        };
        var reservation = new TableReservation
        {
            TenantId = tenantId,
            DiningTable = reservationTable,
            NumberOfGuests = 2,
            ReservationStart = DateTime.UtcNow.AddDays(20),
            ReservationEnd = DateTime.UtcNow.AddDays(20).AddHours(1),
            FirstName = "Foreign",
            LastName = "Guest",
            Email = "foreign@example.test",
            PhoneNumber = "123",
            Status = ReservationStatus.Pending
        };

        db.DiningTables.Add(diningTable);
        db.DiningTables.Add(reservationTable);
        db.MealCategories.Add(mealCategory);
        db.Meals.Add(meal);
        db.InventoryItems.Add(inventoryItem);
        db.ProductCategories.Add(productCategory);
        db.TableReservations.Add(reservation);
        await db.SaveChangesAsync();

        return new CrossTenantResourceIds(
            tenantId,
            restaurantId,
            layout.Id,
            diningTable.Id,
            mealCategory.Id,
            meal.Id,
            inventoryItem.Id,
            productCategory.Id,
            reservation.Id);
    }

    private sealed record CrossTenantResourceIds(
        Guid TenantId,
        Guid RestaurantId,
        int TableLayoutId,
        int DiningTableId,
        int MealCategoryId,
        int MealId,
        int InventoryItemId,
        int ProductCategoryId,
        int TableReservationId);

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

    private static string GetQueryParam(string url, string name)
    {
        var query = new Uri(url).Query.TrimStart('?');
        foreach (var part in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var pair = part.Split('=', 2);
            if (pair.Length == 2 &&
                string.Equals(Uri.UnescapeDataString(pair[0]), name, StringComparison.OrdinalIgnoreCase))
            {
                return Uri.UnescapeDataString(pair[1].Replace('+', ' '));
            }
        }

        throw new InvalidOperationException($"Query parameter '{name}' was not found in '{url}'.");
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
