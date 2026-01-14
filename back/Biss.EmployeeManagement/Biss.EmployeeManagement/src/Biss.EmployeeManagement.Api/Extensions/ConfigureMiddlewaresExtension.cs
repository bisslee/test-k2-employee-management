using Biss.EmployeeManagement.Api.Middleware;
using Biss.EmployeeManagement.Domain.Constants;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Options;

namespace Biss.EmployeeManagement.Api.Extensions
{
    public static class ConfigureMiddlewaresExtension
    {
        public static WebApplication ConfigureMiddlewares(this WebApplication app)
        {
            var localizationOptions = app.Services.GetService<IOptions<RequestLocalizationOptions>>()?.Value;
            if (localizationOptions != null)
            {
                app.UseRequestLocalization(localizationOptions);
            }

            // Adicionar compressão de resposta
            app.UseResponseCompression();

            // Middlewares de logging - ordem é importante!
            // 1. GlobalExceptionHandler PRIMEIRO para tratar exceções antes de qualquer log
            // Isso garante que a resposta seja enviada corretamente antes de qualquer log adicional
            app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
            
            // 2. StructuredLogging para capturar todas as requisições (usa Serilog)
            app.UseStructuredLogging();
            
            // 3. HTTP Logging do ASP.NET Core para logar requisições/respostas
            app.UseHttpLogging();
            
            // Middlewares de segurança
            app.UseMiddleware<SecurityHeadersMiddleware>();
            app.UseMiddleware<RateLimitingMiddleware>();

            // Swagger disponível em todos os ambientes para facilitar testes e documentação
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Biss Employee Management API v1");
                c.RoutePrefix = "swagger";
                c.DocumentTitle = "Biss Employee Management API Documentation";
                c.DefaultModelsExpandDepth(-1);
                c.DisplayRequestDuration();
                c.EnableDeepLinking();
                c.EnableFilter();
                c.ShowExtensions();
                c.EnableValidator();
            });

            // Configurar CORS usando a política configurada
            app.UseCors(ServiceConstants.CorsPolicy);

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            return app;
        }
    }
}
