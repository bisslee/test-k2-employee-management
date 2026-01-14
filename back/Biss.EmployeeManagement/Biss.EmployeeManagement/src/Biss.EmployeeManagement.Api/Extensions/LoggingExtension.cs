using Microsoft.AspNetCore.HttpLogging;
using Serilog;
using Serilog.Context;
using Serilog.Events;

namespace Biss.EmployeeManagement.Api.Extensions
{
    public static class LoggingExtension
    {
        public static IServiceCollection ConfigureLogging(this IServiceCollection services, IConfiguration configuration)
        {
            // Configura o Serilog
            var loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", "Biss.EmployeeManagement.Api")
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
                .WriteTo.File(
                    path: "logs/employee-management-.log",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30,
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}");
            
            // Lê configurações do appsettings.json se existirem
            loggerConfiguration.ReadFrom.Configuration(configuration);
            
            // Cria e registra o logger
            var logger = loggerConfiguration.CreateLogger();
            Log.Logger = logger;
            
            // Configura o Serilog no host
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddSerilog(logger, dispose: true);
            });

            // Configura HTTP logging para capturar requisições e respostas
            services.AddHttpLogging(logging =>
            {
                logging.LoggingFields = HttpLoggingFields.All;
                logging.RequestHeaders.Add("Authorization");
                logging.RequestHeaders.Add("X-Correlation-ID");
                logging.ResponseHeaders.Add("X-Correlation-ID");
                logging.MediaTypeOptions.AddText("application/json");
                logging.RequestBodyLogLimit = 4096;
                logging.ResponseBodyLogLimit = 4096;
            });

            return services;
        }

        public static IApplicationBuilder UseStructuredLogging(this IApplicationBuilder app)
        {
            // Adiciona middleware para capturar correlation ID
            app.Use(async (context, next) =>
            {
                var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() 
                    ?? Guid.NewGuid().ToString();
                
                context.Response.Headers["X-Correlation-ID"] = correlationId;
                
                using (LogContext.PushProperty("CorrelationId", correlationId))
                using (LogContext.PushProperty("RequestPath", context.Request.Path))
                using (LogContext.PushProperty("RequestMethod", context.Request.Method))
                {
                    await next();
                }
            });

            // Adiciona middleware para capturar exceções
            app.Use(async (context, next) =>
            {
                try
                {
                    await next();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Unhandled exception occurred during request processing");
                    throw;
                }
            });

            return app;
        }
    }
}
