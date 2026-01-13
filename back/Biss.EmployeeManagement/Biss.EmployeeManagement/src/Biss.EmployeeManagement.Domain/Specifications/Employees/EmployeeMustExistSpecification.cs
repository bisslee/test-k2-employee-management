using Biss.EmployeeManagement.Domain.Entities;
using Biss.EmployeeManagement.Domain.Exceptions;
using Biss.EmployeeManagement.Domain.Repositories;
using System;
using System.Threading.Tasks;

namespace Biss.EmployeeManagement.Domain.Specifications.Employees
{
    /// <summary>
    /// Specification que valida se o employee existe
    /// </summary>
    public class EmployeeMustExistSpecification : IAsyncSpecification<Guid>
    {
        private readonly IReadRepository<Employee> EmployeeRepository;

        public EmployeeMustExistSpecification(IReadRepository<Employee> employeeRepository)
        {
            EmployeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
        }

        public bool IsSatisfiedBy(Guid employeeId)
        {
            // Para compatibilidade, mas não deve ser usado
            throw new NotImplementedException("Use IsSatisfiedByAsync for async operations");
        }

        public async Task<bool> IsSatisfiedByAsync(Guid employeeId)
        {
            if (employeeId == Guid.Empty)
                throw new EmployeeNotFoundException(employeeId);

            var employee = await EmployeeRepository.GetByIdAsync(employeeId);

            if (employee == null)
                throw new EmployeeNotFoundException(employeeId);

            return true;
        }

        public string ErrorMessage => "Employee not found.";
    }
}
