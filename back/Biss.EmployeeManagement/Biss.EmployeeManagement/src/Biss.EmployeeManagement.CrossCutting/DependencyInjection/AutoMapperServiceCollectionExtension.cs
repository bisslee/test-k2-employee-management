using Biss.EmployeeManagement.Application.Mapping;
using Microsoft.Extensions.DependencyInjection;

namespace Biss.EmployeeManagement.CrossCutting.DependencyInjection
{
    public static class AutoMapperServiceCollectionExtension
    {
        public static IServiceCollection AddAutoMapper(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(MappingConfig));

            return services;
        }
    }
}
