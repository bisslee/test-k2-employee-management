using Biss.EmployeeManagement.Domain.Entities;
using Biss.EmployeeManagement.Domain.Exceptions;
using Biss.EmployeeManagement.Domain.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Biss.EmployeeManagement.Domain.Specifications.Employees
{
    /// <summary>
    /// Specification que valida se o documento do employee é único
    /// </summary>
    public class EmployeeDocumentMustBeUniqueSpecification : IAsyncSpecification<Employee>
    {
        private readonly IReadRepository<Employee> EmployeeRepository;
        private readonly Guid? ExcludeEmployeeId;

        public EmployeeDocumentMustBeUniqueSpecification(IReadRepository<Employee> employeeRepository, Guid? excludeEmployeeId = null)
        {
            EmployeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
            ExcludeEmployeeId = excludeEmployeeId;
        }

        public bool IsSatisfiedBy(Employee employee)
        {
            // Para compatibilidade, mas não deve ser usado
            throw new NotImplementedException("Use IsSatisfiedByAsync for async operations");
        }

        public async Task<bool> IsSatisfiedByAsync(Employee employee)
        {
            if (employee == null || string.IsNullOrWhiteSpace(employee.Document))
                return true;

            var existingEmployees = await EmployeeRepository.Find(e => e.Document == employee.Document);
            var existingEmployee = existingEmployees?.FirstOrDefault();

            if (existingEmployee == null)
                return true;

            if (ExcludeEmployeeId.HasValue && 
                existingEmployee.Id == ExcludeEmployeeId.Value)
                return true;

            throw new EmployeeDocumentAlreadyExistsException(employee.Document, existingEmployee.Id);
        }

        public string ErrorMessage => "Document already exists.";
    }
}
