using Biss.EmployeeManagement.Domain.Entities.Enums;
using MediatR;
using System;
using System.Collections.Generic;

namespace Biss.EmployeeManagement.Application.Commands.Employees.AddEmployee
{
    public class AddEmployeeRequest : IRequest<AddEmployeeResponse>
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Document { get; set; } = null!;
        public DateTime BirthDate { get; set; }
        public EmployeeRole Role { get; set; }
        public string Password { get; set; } = null!;
        public List<PhoneNumberRequest> PhoneNumbers { get; set; } = new List<PhoneNumberRequest>();
    }

    public class PhoneNumberRequest
    {
        public string Number { get; set; } = null!;
        public string? Type { get; set; }
    }
}
