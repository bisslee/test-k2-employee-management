using MediatR;
using System;

namespace Biss.EmployeeManagement.Application.Commands.Employees.RemoveEmployee
{
    public class RemoveEmployeeRequest : IRequest<RemoveEmployeeResponse>
    {
        public Guid Id { get; set; }
    }
}
