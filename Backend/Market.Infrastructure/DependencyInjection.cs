using Market.Application.Abstractions;
using Market.Infrastructure.Common;
using Market.Infrastructure.Database;
using Market.Infrastructure.Identity;
using Market.Shared.Constants;
using Market.Shared.Options;
using Duende.IdentityModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Market.Domain.Entities.IdentityV2;
using Microsoft.AspNetCore.Identity;
using Market.Infrastructure.Database.Seeders;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;


namespace Market.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment env)
    {
        // Typed ConnectionStrings + validation
        services.AddOptions<ConnectionStringsOptions>()
            .Bind(configuration.GetSection(ConnectionStringsOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // DbContext: InMemory for test environments; SQL Server otherwise
        services.AddDbContext<DatabaseContext>((sp, options) =>
        {
            if (env.IsTest())
            {
                options.UseInMemoryDatabase("IntegrationTestsDb");

                return;
            }

            var cs = sp.GetRequiredService<IOptions<ConnectionStringsOptions>>().Value.Main;
            options.UseSqlServer(cs);
        });

        services.AddDbContext<IdentityDatabaseContext>((sp, options) =>
        {
            if (env.IsTest())
            {
                options.UseInMemoryDatabase("IdentityTestsDb");

                return;
            }

            var cs = sp.GetRequiredService<IOptions<ConnectionStringsOptions>>().Value.Main;
            options.UseSqlServer(cs, sql => sql.MigrationsAssembly(typeof(IdentityDatabaseContext).Assembly.FullName));
        });

        // IAppDbContext mapping
        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<DatabaseContext>());

        var identityBuilder = services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 4;
            options.Password.RequiredUniqueChars = 1;

            options.User.RequireUniqueEmail = true;

            options.SignIn.RequireConfirmedEmail = false;

            options.Lockout.AllowedForNewUsers = true;
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        });

        identityBuilder
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<IdentityDatabaseContext>()
            .AddSignInManager()
            .AddRoleManager<RoleManager<ApplicationRole>>()
            .AddDefaultTokenProviders();

        services.Configure<DataProtectionTokenProviderOptions>(opts =>
            opts.TokenLifespan = TimeSpan.FromHours(2));
        services.Configure<SecurityStampValidatorOptions>(opts =>
            opts.ValidationInterval = TimeSpan.FromMinutes(5));

        // Identity hasher
        services.AddScoped<IPasswordHasher<AppUser>, PasswordHasher<AppUser>>();

        var authority = configuration["IdentityServer:Authority"];
        if (string.IsNullOrWhiteSpace(authority))
        {
            authority = env.IsDevelopment()
                ? "https://localhost:7260"
                : throw new InvalidOperationException("IdentityServer:Authority must be configured for non-development environments.");
        }

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddCookie(IdentityConstants.ApplicationScheme, options =>
        {
            options.LoginPath = "/account/login";
            options.LogoutPath = "/account/logout";
            options.AccessDeniedPath = "/account/login";
            options.SlidingExpiration = true;
            options.ExpireTimeSpan = TimeSpan.FromHours(8);
        })
        .AddCookie(IdentityConstants.ExternalScheme, options =>
        {
            options.Cookie.Name = "biteflow.external";
            options.ExpireTimeSpan = TimeSpan.FromMinutes(10);
            options.SlidingExpiration = false;
        })
        .AddGoogle(options =>
        {
            options.SignInScheme = IdentityConstants.ExternalScheme;
            options.ClientId = configuration["Authentication:Google:ClientId"]
                ?? throw new InvalidOperationException("Authentication:Google:ClientId is not configured.");
            options.ClientSecret = configuration["Authentication:Google:ClientSecret"]
                ?? throw new InvalidOperationException("Authentication:Google:ClientSecret is not configured.");
            options.SaveTokens = true;
            options.Scope.Add("email");
            options.Scope.Add("profile");
            options.ClaimActions.MapJsonKey("picture", "picture");
        })
        .AddJwtBearer(options =>
        {
            options.Authority = authority;
            options.RequireHttpsMetadata = !env.IsDevelopment();
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidAudience = "biteflow.api",
                ValidateIssuer = true,
                ValidIssuer = authority,
                NameClaimType = JwtClaimTypes.Name,
                RoleClaimType = JwtClaimTypes.Role,
                ClockSkew = TimeSpan.Zero
            };
            options.MapInboundClaims = false;
        });

        // Token service (reads JwtOptions via IOptions<JwtOptions>)
        services.AddTransient<IJwtTokenService, JwtTokenService>();

        // HttpContext accessor + current user
        services.AddHttpContextAccessor();
        services.AddScoped<IAppCurrentUser, AppCurrentUser>();

        // TimeProvider (if used in handlers/services)
        services.AddSingleton<TimeProvider>(TimeProvider.System);

        services.AddScoped<IdentitySeeder>();
        services.AddScoped<StaffProfileService>();

        return services;
    }
}
