using MediatR;

namespace Biss.EmployeeManagement.Application.Commands.Auth.Login
{
    public class LoginRequest : IRequest<LoginResponse>
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
