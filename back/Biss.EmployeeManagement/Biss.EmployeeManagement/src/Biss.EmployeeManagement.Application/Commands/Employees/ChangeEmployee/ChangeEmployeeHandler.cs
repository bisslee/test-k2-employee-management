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

namespace Biss.EmployeeManagement.Application.Commands.Employees.ChangeEmployee
{
    public class ChangeEmployeeHandler : IRequestHandler<ChangeEmployeeRequest, ChangeEmployeeResponse>
    {
        private readonly ILogger<ChangeEmployeeHandler> Logger;
        private readonly IWriteRepository<Employee> Repository;
        private readonly IReadRepository<Employee> ReadRepository;
        private readonly IValidator<ChangeEmployeeRequest> Validator;
        private readonly IMapper Mapper;
        private readonly IResponseBuilder ResponseBuilder;

        public ChangeEmployeeHandler(
            ILogger<ChangeEmployeeHandler> logger,
            IWriteRepository<Employee> repository,
            IReadRepository<Employee> readRepository,
            IValidator<ChangeEmployeeRequest> validator,
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

        public async Task<ChangeEmployeeResponse> Handle(ChangeEmployeeRequest request, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Starting ChangeEmployeeRequest processing for employee ID: {EmployeeId}, Email: {Email}", 
                request.Id, request.Email);

            var validationResult = await Validator.ValidateAsync(request, cancellationToken);
            if (validationResult != null && !validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage);
                var errorMessage = string.Join("; ", errors);
                Logger.LogWarning("Validation failed for ChangeEmployeeRequest. EmployeeId: {EmployeeId}, Email: {Email}, Errors: {Errors}", 
                    request.Id, request.Email, string.Join(", ", errors));
                return ResponseBuilder.BuildValidationErrorResponse<ChangeEmployeeResponse, Employee>(errorMessage, errors);
            }

            try
            {
                // Validar se employee existe usando Specification
                var mustExistSpecification = new EmployeeMustExistSpecification(ReadRepository);
                await mustExistSpecification.IsSatisfiedByAsync(request.Id);
                
                Logger.LogDebug("Employee exists, retrieving for update. EmployeeId: {EmployeeId}", request.Id);
                var existingEntity = await ReadRepository.GetByIdAsync(request.Id);

                Logger.LogDebug("Employee found, updating data. EmployeeId: {EmployeeId}", request.Id);
                Mapper.Map(request, existingEntity);
                
                // Validar regras de negócio usando Specifications (excluindo o employee atual)
                var emailSpecification = new EmployeeEmailMustBeUniqueSpecification(ReadRepository, request.Id);
                await emailSpecification.IsSatisfiedByAsync(existingEntity);
                
                var documentSpecification = new EmployeeDocumentMustBeUniqueSpecification(ReadRepository, request.Id);
                await documentSpecification.IsSatisfiedByAsync(existingEntity);
                
                existingEntity.UpdatedAt = DateTime.UtcNow;
                existingEntity.UpdatedBy = "System";

                // Atualizar senha se fornecida
                if (!string.IsNullOrEmpty(request.Password))
                {
                    existingEntity.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, BCrypt.Net.BCrypt.GenerateSalt());
                }

                // Atualizar telefones
                existingEntity.PhoneNumbers.Clear();
                foreach (var phoneRequest in request.PhoneNumbers)
                {
                    existingEntity.PhoneNumbers.Add(new PhoneNumber
                    {
                        Id = phoneRequest.Id ?? Guid.NewGuid(),
                        EmployeeId = existingEntity.Id,
                        Number = phoneRequest.Number,
                        Type = phoneRequest.Type ?? "Mobile",
                        UpdatedAt = DateTime.UtcNow,
                        UpdatedBy = "System"
                    });
                }

                Logger.LogDebug("Employee entity mapped successfully. EmployeeId: {EmployeeId}, Email: {Email}", 
                    existingEntity.Id, existingEntity.Email);

                var result = await Repository.Update(existingEntity);

                if (result)
                {
                    Logger.LogInformation("Employee updated successfully. EmployeeId: {EmployeeId}, Email: {Email}", 
                        existingEntity.Id, existingEntity.Email);
                    
                    return ResponseBuilder.BuildSuccessResponse<ChangeEmployeeResponse, Employee>(existingEntity, "Employee updated successfully");
                }

                Logger.LogError("Failed to update employee in repository. EmployeeId: {EmployeeId}, Email: {Email}", 
                    existingEntity.Id, existingEntity.Email);
                return ResponseBuilder.BuildErrorResponse<ChangeEmployeeResponse, Employee>("Failed to update employee");
            }
            catch (EmployeeEmailAlreadyExistsException ex)
            {
                Logger.LogWarning("Employee email already exists during update. EmployeeId: {EmployeeId}, Email: {Email}", request.Id, request.Email);
                return ResponseBuilder.BuildErrorResponse<ChangeEmployeeResponse, Employee>(ex.Message, statusCode: 400);
            }
            catch (EmployeeDocumentAlreadyExistsException ex)
            {
                Logger.LogWarning("Employee document already exists during update. EmployeeId: {EmployeeId}, Document: {Document}", request.Id, request.Document);
                return ResponseBuilder.BuildErrorResponse<ChangeEmployeeResponse, Employee>(ex.Message, statusCode: 400);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Unexpected error occurred while updating employee. EmployeeId: {EmployeeId}, Email: {Email}", 
                    request.Id, request.Email);
                throw; // Deixar o middleware global tratar a exceção
            }
        }
    }
}
