using Biss.EmployeeManagement.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Biss.EmployeeManagement.Application.Services
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly IConfiguration Configuration;
        private readonly ILogger<JwtTokenService> Logger;

        public JwtTokenService(IConfiguration configuration, ILogger<JwtTokenService> logger)
        {
            Configuration = configuration;
            Logger = logger;
        }

        public Task<string> GenerateTokenAsync(Employee employee)
        {
            try
            {
                var jwtSettings = Configuration.GetSection("Security:JwtSettings");
                var secretKey = jwtSettings["SecretKey"];
                var issuer = jwtSettings["Issuer"];
                var audience = jwtSettings["Audience"];
                var expirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"] ?? "60");

                if (string.IsNullOrWhiteSpace(secretKey))
                {
                    Logger.LogError("JWT SecretKey is not configured");
                    throw new InvalidOperationException("JWT SecretKey is not configured");
                }

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, employee.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, employee.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.NameIdentifier, employee.Id.ToString()),
                    new Claim(ClaimTypes.Email, employee.Email),
                    new Claim(ClaimTypes.Name, $"{employee.FirstName} {employee.LastName}"),
                    new Claim("role", employee.Role.ToString()),
                    new Claim("employeeId", employee.Id.ToString())
                };

                var token = new JwtSecurityToken(
                    issuer: issuer,
                    audience: audience,
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
                    signingCredentials: credentials
                );

                var tokenHandler = new JwtSecurityTokenHandler();
                var tokenString = tokenHandler.WriteToken(token);

                Logger.LogInformation("JWT token generated successfully for employee: {EmployeeId}", employee.Id);

                return Task.FromResult(tokenString);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error generating JWT token for employee: {EmployeeId}", employee.Id);
                throw;
            }
        }

        public int GetTokenExpirationMinutes()
        {
            try
            {
                var jwtSettings = Configuration.GetSection("Security:JwtSettings");
                var expirationMinutesStr = jwtSettings["ExpirationMinutes"];
                return string.IsNullOrWhiteSpace(expirationMinutesStr) 
                    ? 60 
                    : int.Parse(expirationMinutesStr);
            }
            catch
            {
                return 60; // Default
            }
        }
    }
}
