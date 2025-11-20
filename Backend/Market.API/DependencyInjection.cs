using Duende.IdentityServer.Services;
using Market.API.Identity;
using Market.Application.Abstractions;
using Market.Infrastructure.Common;
using Market.Shared.Constants;
using Market.Shared.Dtos;
using Market.Shared.Options;
using Microsoft.OpenApi.Models;

namespace Market.API;

public static class DependencyInjection
{
    public static IServiceCollection AddAPI(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment env)
    {
        // Controllers + uniform BadRequest
        services.AddControllers()
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

        // Typed options + validation on startup
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddAuthorization(opt =>
        {
            opt.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();

            opt.AddPolicy(PolicyNames.SuperAdminOnly, policy =>
            {
                policy.RequireRole(RoleNames.SuperAdmin);
            });

            opt.AddPolicy(PolicyNames.RestaurantAdmin, policy =>
            {
                policy.RequireAssertion(ctx =>
                    ctx.User.IsInRole(RoleNames.SuperAdmin) ||
                    ctx.User.IsInRole(RoleNames.Admin)
                    // TODO: implement tenant-specific check when tenant context is available
                );
            });

            opt.AddPolicy(PolicyNames.StaffMember, policy =>
            {
                policy.RequireAssertion(ctx =>
                    ctx.User.IsInRole(RoleNames.SuperAdmin) ||
                    ctx.User.IsInRole(RoleNames.Admin) ||
                    ctx.User.IsInRole(RoleNames.Staff));
            });
        });

        // Swagger with Bearer auth
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Market API", Version = "v1" });
            var xml = Path.Combine(AppContext.BaseDirectory, "Market.API.xml");
            if (File.Exists(xml))
                c.IncludeXmlComments(xml, includeControllerXmlComments: true);

            var bearer = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description = "Unesi JWT token.", // Format: **Bearer {token}** -> Format je samo token Bearer dio se dodaje automatski na swaggeru
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            };
            c.AddSecurityDefinition("Bearer", bearer);
            c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows
                {
                    Password = new OpenApiOAuthFlow
                    {
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

            c.AddSecurityRequirement(new OpenApiSecurityRequirement { { bearer, Array.Empty<string>() } });
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
        return services;
    }



}
