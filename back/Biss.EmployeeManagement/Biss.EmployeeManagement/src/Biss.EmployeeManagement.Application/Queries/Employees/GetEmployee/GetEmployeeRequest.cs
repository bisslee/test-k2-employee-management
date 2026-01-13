using Biss.EmployeeManagement.Application.Queries;
using Biss.EmployeeManagement.Domain.Entities.Enums;
using MediatR;
using System;

namespace Biss.EmployeeManagement.Application.Queries.Employees.GetEmployee
{
    public class GetEmployeeRequest : BaseRequest, IRequest<GetEmployeeResponse>
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Document { get; set; }
        public DateTime? StartBirthDate { get; set; }
        public DateTime? EndBirthDate { get; set; }
        public EmployeeRole? Role { get; set; }
        public bool? IsActive { get; set; }
    }
}
