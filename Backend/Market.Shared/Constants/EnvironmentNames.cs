using Microsoft.Extensions.Hosting;

namespace Market.Shared.Constants;

public static class EnvironmentExtensions
{
    public static bool IsIntegrationTests(this IHostEnvironment env)
        => env.IsEnvironment("IntegrationTests");

    public static bool IsDevelopmentOrStaging(this IHostEnvironment env)
        => env.IsDevelopment() || env.IsStaging();

    public static bool IsProductionOrStaging(this IHostEnvironment env)
        => env.IsProduction() || env.IsStaging();
}
