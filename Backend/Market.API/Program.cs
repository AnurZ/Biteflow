using Market.API;
using Market.Application;
using Market.Infrastructure;

public partial class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Registrations by layers
        builder.Services
            .AddAPI(builder.Configuration, builder.Environment)
            .AddInfrastructure(builder.Configuration, builder.Environment)
            .AddApplication();

        var app = builder.Build();

        // Middleware pipeline
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseMiddleware<ExceptionMiddleware>();

        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        // Migracije + seeding (centralized in Infrastructure)
        await app.Services.InitializeDatabaseAsync(app.Environment);

        app.Run();
    }
}