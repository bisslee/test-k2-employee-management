using Microsoft.Extensions.DependencyInjection;
using System;

namespace Biss.EmployeeManagement.CrossCutting.DependencyInjection
{
    public static class MediatorServiceCollectionExtension
    {
        public static IServiceCollection AddMediator(this IServiceCollection services)
        {
            var assembly = AppDomain.CurrentDomain.Load("Biss.EmployeeManagement.Application");
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

            return services;
        }
    }
}
