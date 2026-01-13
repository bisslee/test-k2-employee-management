using Biss.EmployeeManagement.Application.Validators;
using FluentValidation;

namespace Biss.EmployeeManagement.Application.Commands.Employees.AddEmployee
{
    public class AddEmployeeValidator : AbstractValidator<AddEmployeeRequest>
    {
        public AddEmployeeValidator()
        {
            RuleFor(x => x.FirstName).ValidateName();
            RuleFor(x => x.LastName).ValidateName();
            RuleFor(x => x.Email).ValidateEmail();
            RuleFor(x => x.Document).ValidateDocument();
            RuleFor(x => x.Password).ValidatePassword();
            
            RuleFor(x => x.PhoneNumbers)
                .NotEmpty()
                .WithMessage("At least two phone numbers are required")
                .Must(phones => phones != null && phones.Count >= 2)
                .WithMessage("At least two phone numbers are required");
            
            RuleForEach(x => x.PhoneNumbers)
                .ChildRules(phone =>
                {
                    phone.RuleFor(p => p.Number).ValidatePhoneRequired();
                });
        }
    }
}
