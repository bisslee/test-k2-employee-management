using Biss.EmployeeManagement.Application.Validators;
using FluentValidation;

namespace Biss.EmployeeManagement.Application.Commands.Auth.Login
{
    public class LoginValidator : AbstractValidator<LoginRequest>
    {
        public LoginValidator()
        {
            RuleFor(x => x.Email).ValidateEmail();
            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage("Password is required");
        }
    }
}
