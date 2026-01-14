using Biss.EmployeeManagement.Api.Extensions;
using Serilog;

namespace Biss.EmployeeManagement.Api
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configura o Serilog
            builder.Services.ConfigureLogging(builder.Configuration);
            builder.Host.UseSerilog();

            // Configura os serviços
            builder.Services.ConfigureServices(builder.Configuration);

            var app = builder.Build();

            // Aplica migrations automaticamente na inicialização
            app.ApplyMigrations();

            // Configura Middlewares (inclui logging e exception handling)
            app.ConfigureMiddlewares();

            // Mapear endpoint de health check
            app.MapHealthChecks("/health");

            app.Run();
        }
    }
}
