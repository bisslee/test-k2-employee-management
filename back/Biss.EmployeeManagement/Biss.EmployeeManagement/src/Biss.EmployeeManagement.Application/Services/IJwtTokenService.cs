using Biss.EmployeeManagement.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace Biss.EmployeeManagement.Application.Services
{
    public interface IJwtTokenService
    {
        Task<string> GenerateTokenAsync(Employee employee);
        int GetTokenExpirationMinutes();
    }
}
