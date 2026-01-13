using FluentValidation;
using MediatR;
using Biss.EmployeeManagement.Application.Helpers;
using Biss.EmployeeManagement.Domain.Entities;
using Biss.EmployeeManagement.Domain.Repositories;
using Biss.EmployeeManagement.Domain.Specifications.Employees;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Biss.EmployeeManagement.Application.Commands.Employees.RemoveEmployee
{
    public class RemoveEmployeeHandler : 
        IRequestHandler<RemoveEmployeeRequest, RemoveEmployeeResponse>
    {
        private readonly ILogger<RemoveEmployeeHandler> Logger;
        private readonly IWriteRepository<Employee> Repository;
        private readonly IReadRepository<Employee> ReadRepository;
        private readonly IValidator<RemoveEmployeeRequest> Validator;
        private readonly IResponseBuilder ResponseBuilder;
        
        public RemoveEmployeeHandler(
            ILogger<RemoveEmployeeHandler> logger,
            IWriteRepository<Employee> repository,
            IReadRepository<Employee> readRepository,
            IValidator<RemoveEmployeeRequest> validator,
            IResponseBuilder responseBuilder)
        {
            Logger = logger;
            Repository = repository;
            ReadRepository = readRepository;
            Validator = validator;
            ResponseBuilder = responseBuilder;
        }

        public async Task<RemoveEmployeeResponse> Handle(RemoveEmployeeRequest request, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Starting RemoveEmployeeRequest processing for employee ID: {EmployeeId}", request.Id);

            var validationResult = await Validator.ValidateAsync(request, cancellationToken);
            if (validationResult != null && !validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage);
                var errorMessage = string.Join("; ", errors);
                Logger.LogWarning("Validation failed for RemoveEmployeeRequest. EmployeeId: {EmployeeId}, Errors: {Errors}", 
                    request.Id, string.Join(", ", errors));
                return ResponseBuilder.BuildValidationErrorResponse<RemoveEmployeeResponse, bool>(errorMessage, errors);
            }

            try
            {
                // Validar se employee existe usando Specification
                var mustExistSpecification = new EmployeeMustExistSpecification(ReadRepository);
                await mustExistSpecification.IsSatisfiedByAsync(request.Id);
                
                Logger.LogDebug("Employee exists, retrieving for removal. EmployeeId: {EmployeeId}", request.Id);
                var entity = await ReadRepository.GetByIdAsync(request.Id);
                
                Logger.LogDebug("Proceeding to delete employee. EmployeeId: {EmployeeId}", entity.Id);
                
                var result = await Repository.Delete(entity);
                
                if (result)
                {
                    Logger.LogInformation("Employee removed successfully. EmployeeId: {EmployeeId}", entity.Id);
                    return ResponseBuilder.BuildSuccessResponse<RemoveEmployeeResponse, bool>(true, "Employee removed successfully");
                }

                Logger.LogError("Failed to remove employee from repository. EmployeeId: {EmployeeId}", entity.Id);
                return ResponseBuilder.BuildErrorResponse<RemoveEmployeeResponse, bool>("Failed to remove employee");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Unexpected error occurred while removing employee. EmployeeId: {EmployeeId}", request.Id);
                throw; // Deixar o middleware global tratar a exceção
            }
        }
    }
}
