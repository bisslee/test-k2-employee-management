using Biss.EmployeeManagement.Infrastructure;
using Biss.EmployeeManagement.Infrastructure.Seed;
using Microsoft.EntityFrameworkCore;

namespace Biss.EmployeeManagement.Api.Extensions
{
    public static class MigrationExtension
    {
        public static void ApplyMigrations(this IHost app)
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;
            try
            {
                var context = services.GetRequiredService<AppDbContext>();
                context.Database.Migrate();
                
                // Seed do usuário master após aplicar migrations
                var seedService = services.GetRequiredService<EmployeeSeedService>();
                seedService.SeedMasterUserAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<AppDbContext>>();
                logger.LogError(ex, "Ocorreu um erro ao aplicar as migrações ou criar seed");
            }
        }
    }
}
