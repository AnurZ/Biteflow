using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Market.Infrastructure.Database;

public sealed class IdentityDatabaseContextFactory
    : IDesignTimeDbContextFactory<IdentityDatabaseContext>
{
    public IdentityDatabaseContext CreateDbContext(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            ?? "Development";

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("Main")
            ?? throw new InvalidOperationException("Connection string 'Main' not found.");

        var optionsBuilder = new DbContextOptionsBuilder<IdentityDatabaseContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new IdentityDatabaseContext(optionsBuilder.Options);
    }
}
