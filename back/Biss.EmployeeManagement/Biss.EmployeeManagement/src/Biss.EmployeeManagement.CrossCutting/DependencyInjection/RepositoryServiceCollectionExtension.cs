using Biss.EmployeeManagement.Domain.Entities;
using Biss.EmployeeManagement.Domain.Repositories;
using Biss.EmployeeManagement.Infrastructure.Repositories;
using Biss.EmployeeManagement.Infrastructure.Seed;
using Microsoft.Extensions.DependencyInjection;

namespace Biss.EmployeeManagement.CrossCutting.DependencyInjection
{
    public static class RepositoryServiceCollectionExtension
    {
        public static IServiceCollection AddRepository(this IServiceCollection services)
        {
            services.AddScoped<IReadRepository<Employee>, ReadRepository<Employee>>();
            services.AddScoped<IWriteRepository<Employee>, WriteRepository<Employee>>();
            services.AddScoped<IReadRepository<PhoneNumber>, ReadRepository<PhoneNumber>>();
            services.AddScoped<IWriteRepository<PhoneNumber>, WriteRepository<PhoneNumber>>();
            
            // Registrar serviço de seed
            services.AddScoped<EmployeeSeedService>();
            
            return services;
        }
    }
}
