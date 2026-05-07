using Duende.IdentityServer;
using Duende.IdentityServer.AspNetIdentity;
using Duende.IdentityServer.Services;
using Market.API;
using Market.API.Identity;
using Market.API.Middlewares;
using Market.Application;
using Market.Domain.Entities.IdentityV2;
using Market.Infrastructure;
using Market.Domain.Entities.BlobStorageSettings;
using Market.API.Hubs;
using Market.Shared.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Serilog;

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

            // ---------------- CONFIG ----------------
            builder.Configuration
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("appsettings.secrets.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .AddUserSecrets<Program>();

            // ---------------- SERILOG ----------------
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

            // ---------------- BLOB STORAGE (CLEAN DI) ----------------
            builder.Services
                .AddOptions<BlobStorageSettings>()
                .BindConfiguration("AzureBlobStorage")
                .ValidateOnStart();

            builder.Services.AddSingleton<BlobStorageService>();

            // ---------------- APPLICATION LAYERS ----------------
            builder.Services
                .AddAPI(builder.Configuration, builder.Environment)
                .AddInfrastructure(builder.Configuration, builder.Environment)
                .AddApplication();

            Log.Information("Services registered successfully.");

            // ---------------- IDENTITY SERVER ----------------
            builder.Services
                .AddIdentityServer(options =>
                {
                    options.Events.RaiseSuccessEvents = true;
                    options.Events.RaiseFailureEvents = true;
                    options.Events.RaiseErrorEvents = true;
                    options.EmitStaticAudienceClaim = true;

                    options.Cors.CorsPolicyName = "AllowAngularDev";

                    options.Authentication.CookieAuthenticationScheme = IdentityConstants.ApplicationScheme;
                    options.Authentication.CookieLifetime = TimeSpan.FromHours(8);

                    options.UserInteraction.LoginUrl = "/account/login";
                    options.UserInteraction.LogoutUrl = "/account/logout";
                })
                .AddAspNetIdentity<ApplicationUser>()
                .AddInMemoryIdentityResources(Config.IdentityResources)
                .AddInMemoryApiScopes(Config.ApiScopes)
                .AddInMemoryApiResources(Config.ApiResources)
                .AddInMemoryClients(Config.Clients(builder.Environment.IsTest()))
                .AddDeveloperSigningCredential()
                .AddResourceOwnerValidator<ResourceOwnerPasswordValidator>()
                .AddProfileService<CustomProfileService>();

            Log.Information("IdentityServer configured successfully.");

            // ---------------- CORS ----------------
            var corsSection = builder.Configuration.GetSection("Cors");
            var allowedOrigins = corsSection.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
            var allowedHeaders = corsSection.GetSection("AllowedHeaders").Get<string[]>() ?? Array.Empty<string>();
            var allowedMethods = corsSection.GetSection("AllowedMethods").Get<string[]>() ?? Array.Empty<string>();
            var allowCredentials = corsSection.GetValue<bool>("AllowCredentials");

            static void ConfigureCorsPolicy(
                CorsPolicyBuilder policy,
                string[] origins,
                string[] headers,
                string[] methods,
                bool credentialsAllowed)
            {
                policy.WithOrigins(origins);

                if (headers.Length > 0)
                    policy.WithHeaders(headers);
                else
                    policy.AllowAnyHeader();

                if (methods.Length > 0)
                    policy.WithMethods(methods);
                else
                    policy.AllowAnyMethod();

                if (credentialsAllowed)
                    policy.AllowCredentials();
            }

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAngularDev", policy =>
                {
                    ConfigureCorsPolicy(policy, allowedOrigins, allowedHeaders, allowedMethods, allowCredentials);
                });
            });

            // ---------------- BUILD APP ----------------
            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.OAuthClientId("swagger-ui");
                    options.OAuthScopes("openid", "profile", "email", "roles", "biteflow.api", "offline_access");
                    options.OAuthScopeSeparator(" ");
                    options.OAuthUsePkce();
                });
            }

            app.UseExceptionHandler();
            app.UseMiddleware<RequestResponseLoggingMiddleware>();

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseCors(policy =>
            {
                ConfigureCorsPolicy(policy, allowedOrigins, allowedHeaders, allowedMethods, allowCredentials);
            });

            app.UseIdentityServer();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.MapHub<OrdersHub>("/hubs/orders");

            // ---------------- DB INIT ----------------
            try
            {
                await app.Services.InitializeDatabaseAsync(app.Environment);
                Log.Information("Database initialized successfully.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Database initialization failed.");
                throw;
            }

            Log.Information("Market API started successfully.");
            app.Run();
        }
        catch (HostAbortedException)
        {
            Log.Information("Host aborted by EF Core tooling - ok.");
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