using Biss.EmployeeManagement.Application.Validators;
using FluentValidation;

namespace Biss.EmployeeManagement.Application.Commands.Employees.RemoveEmployee
{
    public class RemoveEmployeeValidator : AbstractValidator<RemoveEmployeeRequest>
    {
        public RemoveEmployeeValidator()
        {
            RuleFor(x => x.Id).ValidateId();
        }
    }
}
