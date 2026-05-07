using Market.Infrastructure.Database;
using Market.Infrastructure.Database.Seeders;
using Market.Infrastructure.Identity;
using Market.Shared.Constants;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Market.Infrastructure;

public static class DatabaseInitializer
{
    /// <summary>
    /// Centralized migration and seeding.
    /// </summary>
    public static async Task InitializeDatabaseAsync(this IServiceProvider services, IHostEnvironment env)
    {
        await using var scope = services.CreateAsyncScope();
        var dbOptions = scope.ServiceProvider.GetRequiredService<DbContextOptions<DatabaseContext>>();
        var clock = scope.ServiceProvider.GetRequiredService<TimeProvider>();
        await using var ctx = new DatabaseContext(dbOptions, clock);
        var identityCtx = scope.ServiceProvider.GetRequiredService<IdentityDatabaseContext>();
        var identitySeeder = scope.ServiceProvider.GetRequiredService<IdentitySeeder>();

        if (env.IsTest())
        {
            await ctx.Database.EnsureCreatedAsync();
            await identityCtx.Database.EnsureCreatedAsync();
            await DynamicDataSeeder.SeedAsync(ctx);
            await identitySeeder.SeedAsync();
            return;
        }

        // SQL Server or similar. Identity tables must exist before app migrations
        // that add foreign keys to AspNetUsers.
        await identityCtx.Database.MigrateAsync();
        await ctx.Database.MigrateAsync();

        if (env.IsDevelopment())
        {
            await DynamicDataSeeder.SeedAsync(ctx);
        }

        await identitySeeder.SeedAsync();
    }
}
