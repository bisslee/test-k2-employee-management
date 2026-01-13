using MediatR;
using Biss.EmployeeManagement.Application.Helpers;
using Biss.EmployeeManagement.Domain.Entities;
using Biss.EmployeeManagement.Domain.Entities.Response;
using Biss.EmployeeManagement.Domain.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Biss.EmployeeManagement.Application.Queries.Employees.GetEmployees
{
    public class GetEmployeesHandler : IRequestHandler<GetEmployeesRequest, GetEmployeesResponse>
    {
        private readonly ILogger<GetEmployeesHandler> Logger;
        private readonly IReadRepository<Employee> Repository;
        private readonly IResponseBuilder ResponseBuilder;

        public GetEmployeesHandler(
            IReadRepository<Employee> repository,
            ILogger<GetEmployeesHandler> logger,
            IResponseBuilder responseBuilder)
        {
            Logger = logger;
            Repository = repository;
            ResponseBuilder = responseBuilder;
        }

        public async Task<GetEmployeesResponse> Handle(
            GetEmployeesRequest request, 
            CancellationToken cancellationToken)
        {
            Logger.LogInformation("Starting GetEmployeesRequest processing");

            try
            {
                request = request.LoadPagination();

                Expression<Func<Employee, bool>> predicate = employee =>
                    (string.IsNullOrEmpty(request.FirstName) || employee.FirstName.Contains(request.FirstName))
                    && (string.IsNullOrEmpty(request.LastName) || employee.LastName.Contains(request.LastName))
                    && (string.IsNullOrEmpty(request.Email) || employee.Email.Contains(request.Email))
                    && (string.IsNullOrEmpty(request.Document) || employee.Document.Contains(request.Document))
                    && ((request.StartBirthDate == null || employee.BirthDate >= request.StartBirthDate)
                        && (request.EndBirthDate == null || employee.BirthDate <= request.EndBirthDate))
                    && (request.Role == null || employee.Role == request.Role)
                    && (request.IsActive == null || employee.IsActive == request.IsActive);

                if (string.IsNullOrEmpty(request.FieldName))
                {
                    request.FieldName = "FirstName";
                }

                var (employees, totalItems) = await Repository.FindWithPagination
                    (
                        predicate,
                        request.Page,
                        request.Offset,
                        request.FieldName,
                        request.Order
                    );

                var response = ResponseBuilder.BuildSuccessResponse<GetEmployeesResponse, List<Employee>>(
                    employees ?? new List<Employee>(), 
                    "Employees retrieved successfully");

                response.Metadata = new ApiMetaDataResponse
                    (
                        totalItems, 
                        request.Offset, 
                        request.Page
                    );

                Logger.LogInformation("GetEmployeesRequest completed. Total: {Total}, Returned: {Returned}", 
                    totalItems, employees?.Count ?? 0);

                return response;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Unexpected error occurred while retrieving employees");
                throw; // Deixar o middleware global tratar a exceção
            }
        }
    }
}
