using Duende.IdentityServer;
using Duende.IdentityServer.AspNetIdentity;
using Duende.IdentityServer.Services;
using Market.API;
using Market.API.Identity;
using Market.API.Middlewares;
using Market.Application;
using Market.Domain.Entities.IdentityV2;
using Market.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Serilog;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.Linq;

public partial class Program
{
    private static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        try
        {
            Log.Information("Starting Market API...");

            var builder = WebApplication.CreateBuilder(args);

            builder.Host.UseSerilog((ctx, services, cfg) =>
            {
                cfg.ReadFrom.Configuration(ctx.Configuration)
                   .ReadFrom.Services(services)
                   .Enrich.FromLogContext()
                   .Enrich.WithThreadId()
                   .Enrich.WithProcessId()
                   .Enrich.WithMachineName();
            });

            builder.Logging.ClearProviders();

            builder.Services
                .AddAPI(builder.Configuration, builder.Environment)
                .AddInfrastructure(builder.Configuration, builder.Environment)
                .AddApplication();

            builder.Services
                .AddIdentityServer(options =>
                {
                    options.Events.RaiseSuccessEvents = true;
                    options.Events.RaiseFailureEvents = true;
                    options.Events.RaiseErrorEvents = true;
                    options.EmitStaticAudienceClaim = true;
                    options.Cors.CorsPolicyName = "AllowAngularDev";
                })
                .AddAspNetIdentity<ApplicationUser>()
                .AddInMemoryIdentityResources(Config.IdentityResources)
                .AddInMemoryApiScopes(Config.ApiScopes)
                .AddInMemoryApiResources(Config.ApiResources)
                .AddInMemoryClients(Config.Clients)
                .AddDeveloperSigningCredential()
                .AddResourceOwnerValidator<LegacyResourceOwnerPasswordValidator>()
                .AddProfileService<CustomProfileService>();

            builder.Services.AddSingleton<ICorsPolicyService>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<DefaultCorsPolicyService>>();
                return new DefaultCorsPolicyService(logger)
                {
                    AllowedOrigins =
                    {
                        "http://localhost:4200",
                        "https://localhost:4200"
                    }
                };
            });

            var allowedOrigins = new[] { "http://localhost:4200", "https://localhost:4200" };

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAngularDev", policy =>
                {
                    policy.WithOrigins(allowedOrigins)
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                });
            });

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseExceptionHandler();
            app.UseMiddleware<RequestResponseLoggingMiddleware>();

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseCors("AllowAngularDev");
            app.Use(async (context, next) =>
            {
                if (HttpMethods.IsOptions(context.Request.Method))
                {
                    var origin = context.Request.Headers["Origin"].ToString();
                    if (!string.IsNullOrWhiteSpace(origin) &&
                        allowedOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase))
                    {
                        context.Response.Headers["Access-Control-Allow-Origin"] = origin;
                        context.Response.Headers["Vary"] = "Origin";
                    }
                    context.Response.Headers["Access-Control-Allow-Credentials"] = "true";

                    var requestMethod = context.Request.Headers["Access-Control-Request-Method"].ToString();
                    if (!string.IsNullOrWhiteSpace(requestMethod))
                    {
                        context.Response.Headers["Access-Control-Allow-Methods"] = requestMethod;
                    }

                    var requestHeaders = context.Request.Headers["Access-Control-Request-Headers"].ToString();
                    if (!string.IsNullOrWhiteSpace(requestHeaders))
                    {
                        context.Response.Headers["Access-Control-Allow-Headers"] = requestHeaders;
                    }
                    else
                    {
                        context.Response.Headers["Access-Control-Allow-Headers"] = "Content-Type, Authorization";
                    }

                    context.Response.StatusCode = StatusCodes.Status204NoContent;
                    await context.Response.CompleteAsync();
                    return;
                }

                var originForResponse = context.Request.Headers["Origin"].ToString();
                if (!string.IsNullOrWhiteSpace(originForResponse) &&
                    allowedOrigins.Contains(originForResponse, StringComparer.OrdinalIgnoreCase))
                {
                    context.Response.OnStarting(() =>
                    {
                        context.Response.Headers["Access-Control-Allow-Origin"] = originForResponse;
                        context.Response.Headers["Access-Control-Allow-Credentials"] = "true";
                        context.Response.Headers.Append("Vary", "Origin");
                        return Task.CompletedTask;
                    });
                }

                await next();
            });
            app.UseIdentityServer();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            await app.Services.InitializeDatabaseAsync(app.Environment);

            Log.Information("Market API started successfully.");
            app.Run();
        }
        catch (HostAbortedException)
        {
            Log.Information("Host aborted by EF Core tooling (design-time) - its ok.");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Market API terminated unexpectedly.");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
