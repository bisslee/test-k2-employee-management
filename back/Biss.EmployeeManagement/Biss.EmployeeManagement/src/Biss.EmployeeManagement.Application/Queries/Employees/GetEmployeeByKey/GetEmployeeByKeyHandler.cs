using MediatR;
using Biss.EmployeeManagement.Application.Helpers;
using Biss.EmployeeManagement.Domain.Entities;
using Biss.EmployeeManagement.Domain.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Biss.EmployeeManagement.Application.Queries.Employees.GetEmployeeByKey
{
    public class GetEmployeeByKeyHandler : IRequestHandler<GetEmployeeByKeyRequest, GetEmployeeByKeyResponse>
    {
        private readonly ILogger<GetEmployeeByKeyHandler> Logger;
        private readonly IReadRepository<Employee> Repository;
        private readonly IResponseBuilder ResponseBuilder;

        public GetEmployeeByKeyHandler(
            IReadRepository<Employee> repository,
            ILogger<GetEmployeeByKeyHandler> logger,
            IResponseBuilder responseBuilder)
        {
            Logger = logger;
            Repository = repository;
            ResponseBuilder = responseBuilder;
        }

        public async Task<GetEmployeeByKeyResponse> Handle(GetEmployeeByKeyRequest request, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Starting GetEmployeeByKeyRequest processing for employee ID: {EmployeeId}", request.Id);

            try
            {
                var result = await Repository.GetByIdAsync(request.Id);
                
                if (result == null)
                {
                    Logger.LogWarning("Employee not found. EmployeeId: {EmployeeId}", request.Id);
                    return ResponseBuilder.BuildNotFoundResponse<GetEmployeeByKeyResponse, Employee>("Employee not found");
                }

                Logger.LogInformation("Employee retrieved successfully. EmployeeId: {EmployeeId}, Email: {Email}", 
                    result.Id, result.Email);

                return ResponseBuilder.BuildSuccessResponse<GetEmployeeByKeyResponse, Employee>(result, "Employee retrieved successfully");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Unexpected error occurred while retrieving employee. EmployeeId: {EmployeeId}", request.Id);
                throw; // Deixar o middleware global tratar a exceção
            }
        }
    }
}
