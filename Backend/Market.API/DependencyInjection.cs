using Duende.IdentityServer.Services;
using Market.API.Identity;
using Market.Application.Abstractions;
using Market.Infrastructure.Common;
using Market.Shared.Constants;
using Market.Shared.Dtos;
using Market.Shared.Options;
using Microsoft.OpenApi.Models;
using Market.API.Options;
using Market.API.Services;

namespace Market.API;

public static class DependencyInjection
{
    public static IServiceCollection AddAPI(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment env)
    {
        // Controllers + uniform BadRequest
        services.AddControllersWithViews()
            .ConfigureApiBehaviorOptions(opts =>
            {
                opts.InvalidModelStateResponseFactory = ctx =>
                {
                    var msg = string.Join("; ",
                        ctx.ModelState.Values.SelectMany(v => v.Errors)
                                             .Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage)
                                                 ? "Validation error"
                                                 : e.ErrorMessage));
                    return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(new ErrorDto
                    {
                        Code = "validation.failed",
                        Message = msg
                    });
                };
            });

        services.AddSignalR();

        services.AddOptions<CaptchaOptions>()
            .Bind(configuration.GetSection(CaptchaOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddAuthorization(opt =>
        {
            opt.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();

            opt.AddPolicy(PolicyNames.SuperAdminOnly, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireRole(RoleNames.SuperAdmin);
            });

            opt.AddPolicy(PolicyNames.RestaurantAdmin, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireAssertion(ctx =>
                    ctx.User.IsInRole(RoleNames.SuperAdmin) ||
                    (ctx.User.IsInRole(RoleNames.Admin) && HasTenantScope(ctx.User))
                    // Resource tenant ownership is enforced by EF query filters and explicit handler checks.
                );
            });

            opt.AddPolicy(PolicyNames.StaffMember, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireAssertion(ctx =>
                    ctx.User.IsInRole(RoleNames.SuperAdmin) ||
                    (HasTenantScope(ctx.User) &&
                     (ctx.User.IsInRole(RoleNames.Admin) ||
                      ctx.User.IsInRole(RoleNames.Staff) ||
                      ctx.User.IsInRole(RoleNames.Waiter) ||
                      ctx.User.IsInRole(RoleNames.Kitchen))));
            });
        });

        // Swagger with OAuth2 (authorization code)
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Market API", Version = "v1" });
            var xml = Path.Combine(AppContext.BaseDirectory, "Market.API.xml");
            if (File.Exists(xml))
                c.IncludeXmlComments(xml, includeControllerXmlComments: true);

            c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows
                {
                    AuthorizationCode = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri("https://localhost:7260/connect/authorize"),
                        TokenUrl = new Uri("https://localhost:7260/connect/token"),
                        Scopes = new Dictionary<string, string>
            {
                { "openid", "OpenID" },
                { "profile", "Profile" },
                { "email", "Email" },
                { "roles", "Roles" },
                { "biteflow.api", "Biteflow API" }
            }
                    }
                }
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" }
                        },
                        new[] { "biteflow.api", "roles", "openid", "profile", "email" }
                    }
                });
        });

        services.AddExceptionHandler<MarketExceptionHandler>();
        services.AddProblemDetails();

        services.Configure<ActivationLinkOptions>(configuration.GetSection("ActivationLink"));
        services.AddScoped<IActivationLinkService, ActivationLinkService>();
        services.AddScoped<IStaffIdentityTerminationService, StaffIdentityTerminationService>();
        services.AddHttpClient();
        services.AddScoped<ICaptchaVerifier, HcaptchaVerifier>();
        services.AddScoped<IOrderExportService, OrderExportService>();
        return services;
    }

    private static bool HasTenantScope(System.Security.Claims.ClaimsPrincipal user)
    {
        var raw = user.FindFirst("tenant_id")?.Value;
        return Guid.TryParse(raw, out var tenantId) && tenantId != Guid.Empty;
    }


}
