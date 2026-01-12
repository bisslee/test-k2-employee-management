using MediatR;
using Biss.EmployeeManagement.Application.Helpers;
using Biss.EmployeeManagement.Domain.Constants;
using Biss.EmployeeManagement.Domain.Entities;
using Biss.EmployeeManagement.Domain.Entities.Response;
using Biss.EmployeeManagement.Domain.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Biss.EmployeeManagement.Application.Queries.GetCustomerByKey
{
    public class GetCustomerByKeyHandler : IRequestHandler<GetCustomerByKeyRequest, GetCustomerByKeyResponse>
    {
        private ILogger<GetCustomerByKeyHandler> Logger { get; }
        private IReadRepository<Customer> Repository { get; }

        private readonly HandleResponseExceptionHelper HandleResponseExceptionHelper;


        public GetCustomerByKeyHandler(

            IReadRepository<Customer> repository,
            ILogger<GetCustomerByKeyHandler> logger
            )
        {
            Logger = logger;
            Repository = repository;
            HandleResponseExceptionHelper = new HandleResponseExceptionHelper(logger);

        }

        public async Task<GetCustomerByKeyResponse> Handle(GetCustomerByKeyRequest request, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Handling GetCustomerRequest");
            var response = new GetCustomerByKeyResponse();
            try
            {
                var result = await Repository.GetByIdAsync(request.Id);
                if (result != null)
                {
                    response.Data = new ApiDataResponse<Customer>(result);
                }
            }
            catch (Exception ex)
            {
                HandleResponseExceptionHelper.HandleResponseException(response, ex);
            }

            Logger.LogInformation($"{
                ServiceConstants.CustomerService} Finish GetCustomerByKeyHandler request");
            return response;
        }
    }
}
