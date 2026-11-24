using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace MarcasAutos.Api.Data;

public static class DatabaseMigrationExtensions
{
    private const int MaxAttempts = 5;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(2);

    public static async Task ApplyMigrationsAsync(this WebApplication app)
    {
        var logger = app.Services
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger(nameof(DatabaseMigrationExtensions));

        await using var scope = app.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<MarcasAutosDbContext>();

        if (!context.Database.IsRelational())
        {
            return;
        }

        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await context.Database.MigrateAsync();
                logger.LogInformation("Migraciones de EF Core aplicadas.");
                return;
            }
            catch (NpgsqlException exception) when (attempt < MaxAttempts)
            {
                logger.LogWarning(
                    exception,
                    "PostgreSQL no aceptó la migración. Intento {Attempt} de {MaxAttempts}.",
                    attempt,
                    MaxAttempts);

                await Task.Delay(RetryDelay);
            }
        }
    }
}
