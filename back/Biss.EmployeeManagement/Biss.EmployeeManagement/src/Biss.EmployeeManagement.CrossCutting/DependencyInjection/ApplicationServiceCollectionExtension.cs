using Biss.EmployeeManagement.Application.Helpers;
using Biss.EmployeeManagement.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Biss.EmployeeManagement.CrossCutting.DependencyInjection
{
    public static class ApplicationServiceCollectionExtension
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IResponseBuilder, ResponseBuilder>();
            services.AddScoped<IJwtTokenService, JwtTokenService>();

            return services;
        }
    }
}
