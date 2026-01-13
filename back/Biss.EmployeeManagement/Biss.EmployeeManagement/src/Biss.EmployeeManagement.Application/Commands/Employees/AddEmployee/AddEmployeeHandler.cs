using AutoMapper;
using FluentValidation;
using MediatR;
using Biss.EmployeeManagement.Application.Helpers;
using Biss.EmployeeManagement.Domain.Entities;
using Biss.EmployeeManagement.Domain.Exceptions;
using Biss.EmployeeManagement.Domain.Repositories;
using Biss.EmployeeManagement.Domain.Specifications.Employees;
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
            catch (Exception ex)
            {
                Logger.LogError(ex, "Unexpected error occurred while creating employee. Email: {Email}", request.Email);
                throw; // Deixar o middleware global tratar a exceção
            }
        }
    }
}
