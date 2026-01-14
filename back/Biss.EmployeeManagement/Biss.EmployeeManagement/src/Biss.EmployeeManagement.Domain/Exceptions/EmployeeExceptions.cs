using System;
using Biss.EmployeeManagement.Domain.Resources;

namespace Biss.EmployeeManagement.Domain.Exceptions
{
    public class EmployeeNotFoundException : DomainException
    {
        public EmployeeNotFoundException(Guid employeeId) 
            : base(string.Format(ResourceApp.EMPLOYEE_NOT_FOUND, employeeId), "EMPLOYEE_NOT_FOUND", 404)
        {
        }

        public EmployeeNotFoundException(string email) 
            : base(string.Format(ResourceApp.EMPLOYEE_NOT_FOUND, email), "EMPLOYEE_NOT_FOUND", 404)
        {
        }
    }

    public class EmployeeEmailAlreadyExistsException : DomainException
    {
        public EmployeeEmailAlreadyExistsException(string email) 
            : base(string.Format(ResourceApp.EMPLOYEE_EMAIL_ALREADY_EXISTS, email), "EMPLOYEE_EMAIL_ALREADY_EXISTS", 400)
        {
        }

        public EmployeeEmailAlreadyExistsException(string email, Guid existingEmployeeId) 
            : base(string.Format(ResourceApp.EMPLOYEE_EMAIL_ALREADY_EXISTS_WITH_ID, email, existingEmployeeId), "EMPLOYEE_EMAIL_ALREADY_EXISTS", 400)
        {
        }
    }

    public class EmployeeDocumentAlreadyExistsException : DomainException
    {
        public EmployeeDocumentAlreadyExistsException(string documentNumber) 
            : base(string.Format(ResourceApp.EMPLOYEE_DOCUMENT_ALREADY_EXISTS, documentNumber), "EMPLOYEE_DOCUMENT_ALREADY_EXISTS", 400)
        {
        }

        public EmployeeDocumentAlreadyExistsException(string documentNumber, Guid existingEmployeeId) 
            : base(string.Format(ResourceApp.EMPLOYEE_DOCUMENT_ALREADY_EXISTS_WITH_ID, documentNumber, existingEmployeeId), "EMPLOYEE_DOCUMENT_ALREADY_EXISTS", 400)
        {
        }
    }

    public class EmployeeValidationException : DomainException
    {
        public EmployeeValidationException(string message) 
            : base(message, "EMPLOYEE_VALIDATION_ERROR", 400)
        {
        }

        public EmployeeValidationException(string message, Exception innerException) 
            : base(message, "EMPLOYEE_VALIDATION_ERROR", 400, innerException)
        {
        }
    }

    public class EmployeeRoleHierarchyException : DomainException
    {
        public EmployeeRoleHierarchyException(string message) 
            : base(message, "EMPLOYEE_ROLE_HIERARCHY_ERROR", 400)
        {
        }
    }

    public class EmployeeOperationException : DomainException
    {
        public EmployeeOperationException(string operation, string message) 
            : base($"Error during employee {operation}: {message}", "EMPLOYEE_OPERATION_ERROR", 500)
        {
        }

        public EmployeeOperationException(string operation, string message, Exception innerException) 
            : base($"Error during employee {operation}: {message}", "EMPLOYEE_OPERATION_ERROR", 500, innerException)
        {
        }
    }
}
