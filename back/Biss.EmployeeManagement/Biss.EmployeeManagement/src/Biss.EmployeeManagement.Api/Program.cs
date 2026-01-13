using Biss.MultiSinkLogger;
using Biss.MultiSinkLogger.ExceptionHandlers;
using Biss.MultiSinkLogger.Http;
using Biss.MultiSinkLogger.Extensions;
using Biss.EmployeeManagement.Api.Extensions;
using Serilog;

namespace Biss.EmployeeManagement.Api
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Inicializa o logger com configurações do appsettings.json
            LoggingManager.InitializeLogger(builder.Configuration);

            // Configura o Serilog
            builder.Host.UseSerilog();

            // Adiciona o HttpClient com o HttpLoggingHandler
            builder.Services.AddTransient<HttpLoggingHandler>();
            builder.Services.AddTransient<IExceptionHandler, DefaultExceptionHandler>();

            // Configura os serviços
            builder.Services.ConfigureServices(builder.Configuration);

            var app = builder.Build();

            // Middlewares para captura de logs
            app.UseExceptionLogging();
            app.UseCustomLogging();

            // Aplica migrations automaticamente na inicialização
            app.ApplyMigrations();

            // Configura Middlewares
            app.ConfigureMiddlewares();

            // Mapear endpoint de health check
            app.MapHealthChecks("/health");

            app.Run();
        }
    }
}
