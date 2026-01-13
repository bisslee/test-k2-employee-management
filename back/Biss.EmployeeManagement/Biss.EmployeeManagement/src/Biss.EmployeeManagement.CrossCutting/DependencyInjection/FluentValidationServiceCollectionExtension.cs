using Biss.EmployeeManagement.Application.Commands.Employees.AddEmployee;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Biss.EmployeeManagement.CrossCutting.DependencyInjection
{
    public static class FluentValidationServiceCollectionExtension
    {
        public static IServiceCollection AddValidators(this IServiceCollection services)
        {
            // Registra todos os validadores da assembly do application
            services
                .AddValidatorsFromAssemblyContaining<AddEmployeeRequest>();

            return services;
        }
    }
}
