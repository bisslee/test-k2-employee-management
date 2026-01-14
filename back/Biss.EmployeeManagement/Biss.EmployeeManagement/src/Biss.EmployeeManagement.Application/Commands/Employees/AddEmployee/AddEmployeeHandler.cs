using AutoMapper;
using FluentValidation;
using MediatR;
using Biss.EmployeeManagement.Application.Helpers;
using Biss.EmployeeManagement.Domain.Entities;
using Biss.EmployeeManagement.Domain.Exceptions;
using Biss.EmployeeManagement.Domain.Repositories;
using Biss.EmployeeManagement.Domain.Specifications.Employees;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Biss.EmployeeManagement.Application.Commands.Employees.AddEmployee
{
    public class AddEmployeeHandler : IRequestHandler<AddEmployeeRequest, AddEmployeeResponse>
    {
        private readonly ILogger<AddEmployeeHandler> Logger;
        private readonly IWriteRepository<Employee> Repository;
        private readonly IReadRepository<Employee> ReadRepository;
        private readonly IValidator<AddEmployeeRequest> Validator;
        private readonly IMapper Mapper;
        private readonly IResponseBuilder ResponseBuilder;

        public AddEmployeeHandler(
            ILogger<AddEmployeeHandler> logger,
            IWriteRepository<Employee> repository,
            IReadRepository<Employee> readRepository,
            IValidator<AddEmployeeRequest> validator,
            IMapper mapper,
            IResponseBuilder responseBuilder)
        {
            Logger = logger;
            Repository = repository;
            ReadRepository = readRepository;
            Validator = validator;
            Mapper = mapper;
            ResponseBuilder = responseBuilder;
        }

        public async Task<AddEmployeeResponse> Handle(AddEmployeeRequest request, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Starting AddEmployeeRequest processing for email: {Email}", request.Email);

            var validationResult = await Validator.ValidateAsync(request, cancellationToken);

            if (validationResult != null && !validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage);
                var errorMessage = string.Join("; ", errors);
                Logger.LogWarning("Validation failed for AddEmployeeRequest. Email: {Email}, Errors: {Errors}",
                    request.Email, string.Join(", ", errors));
                return ResponseBuilder.BuildValidationErrorResponse<AddEmployeeResponse, Employee>(errorMessage, errors);
            }

            try
            {
                var entity = Mapper.Map<Employee>(request);
                
                // Normalizar documento antes de validar e salvar (remover formatação)
                entity.Document = NormalizeDocument(entity.Document);
                
                // Validar regras de negócio usando Specifications
                var emailSpecification = new EmployeeEmailMustBeUniqueSpecification(ReadRepository);
                await emailSpecification.IsSatisfiedByAsync(entity);
                
                var documentSpecification = new EmployeeDocumentMustBeUniqueSpecification(ReadRepository);
                await documentSpecification.IsSatisfiedByAsync(entity);
                entity.Id = Guid.NewGuid();
                entity.CreatedAt = DateTime.UtcNow;
                entity.CreatedBy = "System";
                
                // Hash da senha com BCrypt
                entity.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, BCrypt.Net.BCrypt.GenerateSalt());

                // Mapear telefones
                foreach (var phoneRequest in request.PhoneNumbers)
                {
                    entity.PhoneNumbers.Add(new PhoneNumber
                    {
                        Id = Guid.NewGuid(),
                        EmployeeId = entity.Id,
                        Number = phoneRequest.Number,
                        Type = phoneRequest.Type ?? "Mobile",
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "System"
                    });
                }

                Logger.LogDebug("Employee entity mapped successfully. ID: {EmployeeId}, Email: {Email}",
                    entity.Id, entity.Email);

                var result = await Repository.Add(entity);

                if (result)
                {
                    Logger.LogInformation("Employee created successfully. ID: {EmployeeId}, Email: {Email}",
                        entity.Id, entity.Email);
                    
                    return ResponseBuilder.BuildSuccessResponse<AddEmployeeResponse, Employee>(entity, "Employee created successfully", 201);
                }

                Logger.LogError("Failed to create employee in repository. ID: {EmployeeId}, Email: {Email}",
                    entity.Id, entity.Email);

                return ResponseBuilder.BuildErrorResponse<AddEmployeeResponse, Employee>("Failed to create employee");
            }
            catch (EmployeeEmailAlreadyExistsException ex)
            {
                Logger.LogWarning("Employee email already exists. Email: {Email}", request.Email);
                return ResponseBuilder.BuildErrorResponse<AddEmployeeResponse, Employee>(ex.Message, statusCode: 400);
            }
            catch (EmployeeDocumentAlreadyExistsException ex)
            {
                Logger.LogWarning("Employee document already exists. Document: {Document}", request.Document);
                return ResponseBuilder.BuildErrorResponse<AddEmployeeResponse, Employee>(ex.Message, statusCode: 400);
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx) when (dbEx.InnerException?.Message?.Contains("UNIQUE constraint") == true || 
                                                                               dbEx.InnerException?.Message?.Contains("Cannot insert duplicate") == true ||
                                                                               dbEx.InnerException?.Message?.Contains("duplicate key") == true)
            {
                // Capturar violação de índice único do banco de dados
                // Isso pode acontecer se a validação não detectou o duplicado (race condition)
                var errorMessage = "O documento ou email informado já está em uso.";
                var innerMessage = dbEx.InnerException?.Message ?? dbEx.Message;
                
                if (innerMessage.Contains("Document") == true || 
                    innerMessage.Contains("IX_Employees_Document") == true ||
                    innerMessage.Contains("Document") == true)
                {
                    errorMessage = "O documento informado já está cadastrado.";
                }
                else if (innerMessage.Contains("Email") == true || 
                         innerMessage.Contains("IX_Employees_Email") == true)
                {
                    errorMessage = "O email informado já está cadastrado.";
                }
                
                Logger.LogWarning("Database unique constraint violation. Email: {Email}, Document: {Document}, InnerException: {InnerException}", 
                    request.Email, request.Document, innerMessage);
                return ResponseBuilder.BuildErrorResponse<AddEmployeeResponse, Employee>(errorMessage, statusCode: 400);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Unexpected error occurred while creating employee. Email: {Email}, Document: {Document}", 
                    request.Email, request.Document);
                return ResponseBuilder.BuildErrorResponse<AddEmployeeResponse, Employee>(
                    "Ocorreu um erro ao criar o funcionário. Tente novamente.", statusCode: 500);
            }
        }

        private static string NormalizeDocument(string document)
        {
            if (string.IsNullOrWhiteSpace(document))
                return string.Empty;

            // Remover pontos, traços, espaços e barras - manter apenas dígitos
            return new string(document.Where(char.IsDigit).ToArray());
        }
    }
}
