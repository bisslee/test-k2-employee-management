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

            // Normalizar documento removendo formatação (pontos, traços, espaços)
            var normalizedDocument = NormalizeDocument(employee.Document);

            // Buscar employees e normalizar documentos para comparação
            // Buscar por documentos que começam com os primeiros dígitos para otimizar
            var allEmployees = await EmployeeRepository.Find(e => !string.IsNullOrWhiteSpace(e.Document));
            var existingEmployee = allEmployees?
                .FirstOrDefault(e => NormalizeDocument(e.Document) == normalizedDocument);

            if (existingEmployee == null)
                return true;

            if (ExcludeEmployeeId.HasValue && 
                existingEmployee.Id == ExcludeEmployeeId.Value)
                return true;

            throw new EmployeeDocumentAlreadyExistsException(employee.Document, existingEmployee.Id);
        }

        private static string NormalizeDocument(string document)
        {
            if (string.IsNullOrWhiteSpace(document))
                return string.Empty;

            // Remover pontos, traços, espaços e barras
            return new string(document.Where(char.IsDigit).ToArray());
        }

        public string ErrorMessage => "Document already exists.";
    }
}
