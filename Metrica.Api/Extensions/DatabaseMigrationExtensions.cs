using Metrica.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Metrica.Api.Extensions
{
    public static class DatabaseMigrationExtensions
    {
        public static async Task ApplyDatabaseMigrationsAsync(
            this WebApplication app)
        {
            await using var scope = app.Services.CreateAsyncScope();

            var dbContext = scope.ServiceProvider
                .GetRequiredService<MetricaDbContext>();

            await dbContext.Database.MigrateAsync();
        }
    }
}