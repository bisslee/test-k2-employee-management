using AutoMapper;
using MediatR;
using Biss.EmployeeManagement.Application.Helpers;
using Biss.EmployeeManagement.Application.Queries.GetCustomerByKey;
using Biss.EmployeeManagement.Domain.Constants;
using Biss.EmployeeManagement.Domain.Entities;
using Biss.EmployeeManagement.Domain.Entities.Response;
using Biss.EmployeeManagement.Domain.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Biss.EmployeeManagement.Application.Commands.RemoveCustomer
{
    public class RemoveCustomerHandler : 
        IRequestHandler<RemoveCustomerRequest, RemoveCustomerResponse>
    {
        private ILogger<RemoveCustomerHandler> Logger { get; }
        private IWriteRepository<Customer> Repository { get; }
        private IMapper Mapper { get; }
        private IMediator Mediator { get; }

        private readonly HandleResponseExceptionHelper HandleResponseExceptionHelper;


        public RemoveCustomerHandler(

            IWriteRepository<Customer> repository,
            ILogger<RemoveCustomerHandler> logger,
            IMapper mapper,
            IMediator mediator
            )
        {
            Logger = logger;
            Repository = repository;
            Mapper = mapper;
            Mediator = mediator;
            HandleResponseExceptionHelper = new HandleResponseExceptionHelper(logger);
        }

        public async Task<RemoveCustomerResponse> Handle(RemoveCustomerRequest request, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Handling GetCustomerRequest");
            var response = new RemoveCustomerResponse();

            try
            {
                var getEntity = new GetCustomerByKeyRequest
                {
                    Id = request.Id
                };

                var getEntityResponse = await Mediator.Send(getEntity);

                if (getEntityResponse.Data == null)
                {
                    response.Error = new ApiErrorResponse
                    {
                        Message = "Customer not found",
                        Detail = "Customer not found"
                    };
                    response.StatusCode = 204;
                    response.Success = false;
                    return response;
                }
                else if (getEntityResponse.Error != null)
                {
                    response.Error = getEntityResponse.Error;
                    response.StatusCode = 500;
                    response.Success = false;
                    return response;
                }

                var result = await Repository.Delete(getEntityResponse.Data.Response);

                if (result)
                {
                    response.Data = new ApiDataResponse<bool>(result);
                    response.Success = true;
                    response.StatusCode = 200;
                }

            }
            catch (Exception ex)
            {
                HandleResponseExceptionHelper.HandleResponseException(response, ex);
            }

            Logger.LogInformation($"{ServiceConstants.CustomerService} Finish Handle request");
            return response;
        }
    }
}
