using Biss.EmployeeManagement.Domain.Entities;
using Biss.EmployeeManagement.Domain.Repositories;
using Biss.EmployeeManagement.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Biss.EmployeeManagement.CrossCutting.DependencyInjection
{
    public static class RepositoryServiceCollectionExtension
    {
        public static IServiceCollection AddRepository(this IServiceCollection services)
        {
            services.AddScoped<IReadRepository<Customer>, ReadRepository<Customer>>();
            services.AddScoped<IWriteRepository<Customer>, WriteRepository<Customer>>();
            return services;
        }
    }
}
