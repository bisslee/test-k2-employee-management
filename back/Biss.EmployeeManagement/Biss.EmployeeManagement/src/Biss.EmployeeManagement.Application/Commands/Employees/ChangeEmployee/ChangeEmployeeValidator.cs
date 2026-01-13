using Biss.EmployeeManagement.Application.Validators;
using FluentValidation;

namespace Biss.EmployeeManagement.Application.Commands.Employees.ChangeEmployee
{
    public class ChangeEmployeeValidator : AbstractValidator<ChangeEmployeeRequest>
    {
        public ChangeEmployeeValidator()
        {
            RuleFor(x => x.Id).ValidateId();
            RuleFor(x => x.FirstName).ValidateName();
            RuleFor(x => x.LastName).ValidateName();
            RuleFor(x => x.Email).ValidateEmail();
            RuleFor(x => x.Document).ValidateDocument();
            
            RuleFor(x => x.Password)
                .Must(password => string.IsNullOrEmpty(password) || password.Length >= 8)
                .WithMessage("Password must be at least 8 characters long if provided");
            
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
