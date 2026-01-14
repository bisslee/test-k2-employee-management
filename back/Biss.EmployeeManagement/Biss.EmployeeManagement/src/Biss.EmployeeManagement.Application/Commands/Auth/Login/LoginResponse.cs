using Biss.EmployeeManagement.Domain.Entities.Response;
using System;

namespace Biss.EmployeeManagement.Application.Commands.Auth.Login
{
    public class LoginResponse : ApiResponse<LoginData>
    {
    }

    public class LoginData
    {
        public string Token { get; set; } = string.Empty;
        public string? RefreshToken { get; set; }
        public DateTime ExpiresAt { get; set; }
        public EmployeeInfo? Employee { get; set; }
    }

    public class EmployeeInfo
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
