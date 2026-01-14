using Biss.EmployeeManagement.Application.Helpers;
using Biss.EmployeeManagement.Application.Services;
using Biss.EmployeeManagement.Domain.Entities;
using Biss.EmployeeManagement.Domain.Repositories;
using BCrypt.Net;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Biss.EmployeeManagement.Application.Commands.Auth.Login
{
    public class LoginHandler : IRequestHandler<LoginRequest, LoginResponse>
    {
        private readonly ILogger<LoginHandler> Logger;
        private readonly IReadRepository<Employee> ReadRepository;
        private readonly IValidator<LoginRequest> Validator;
        private readonly IResponseBuilder ResponseBuilder;
        private readonly IJwtTokenService JwtTokenService;

        public LoginHandler(
            ILogger<LoginHandler> logger,
            IReadRepository<Employee> readRepository,
            IValidator<LoginRequest> validator,
            IResponseBuilder responseBuilder,
            IJwtTokenService jwtTokenService)
        {
            Logger = logger;
            ReadRepository = readRepository;
            Validator = validator;
            ResponseBuilder = responseBuilder;
            JwtTokenService = jwtTokenService;
        }

        public async Task<LoginResponse> Handle(LoginRequest request, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Starting LoginRequest processing for email: {Email}", request.Email);

            var validationResult = await Validator.ValidateAsync(request, cancellationToken);
            if (validationResult != null && !validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage);
                var errorMessage = string.Join("; ", errors);
                Logger.LogWarning("Validation failed for LoginRequest. Email: {Email}, Errors: {Errors}",
                    request.Email, string.Join(", ", errors));
                return ResponseBuilder.BuildValidationErrorResponse<LoginResponse, LoginData>(errorMessage, errors);
            }

            try
            {
                // Buscar funcionário por email
                var employees = await ReadRepository.Find(e => e.Email == request.Email);
                var employee = employees?.FirstOrDefault();

                if (employee == null)
                {
                    Logger.LogWarning("Employee not found for login. Email: {Email}", request.Email);
                    return ResponseBuilder.BuildErrorResponse<LoginResponse, LoginData>("Invalid email or password", statusCode: 401);
                }

                // Verificar senha
                if (!BCrypt.Net.BCrypt.Verify(request.Password, employee.PasswordHash))
                {
                    Logger.LogWarning("Invalid password for login. Email: {Email}", request.Email);
                    return ResponseBuilder.BuildErrorResponse<LoginResponse, LoginData>("Invalid email or password", statusCode: 401);
                }

                // Gerar JWT token
                var token = await JwtTokenService.GenerateTokenAsync(employee);
                
                // Obter expiração do token das configurações
                var expirationMinutes = JwtTokenService.GetTokenExpirationMinutes();

                var loginData = new LoginData
                {
                    Token = token,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes),
                    Employee = new EmployeeInfo
                    {
                        Id = employee.Id,
                        FirstName = employee.FirstName,
                        LastName = employee.LastName,
                        Email = employee.Email,
                        Role = employee.Role.ToString()
                    }
                };

                Logger.LogInformation("Login successful. Email: {Email}, EmployeeId: {EmployeeId}", 
                    request.Email, employee.Id);

                return ResponseBuilder.BuildSuccessResponse<LoginResponse, LoginData>(loginData, "Login successful");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Unexpected error occurred while processing login. Email: {Email}", request.Email);
                throw; // Deixar o middleware global tratar a exceção
            }
        }
    }
}
