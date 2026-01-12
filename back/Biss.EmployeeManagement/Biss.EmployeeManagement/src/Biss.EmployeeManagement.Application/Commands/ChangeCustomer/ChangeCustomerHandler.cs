using AutoMapper;
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

namespace Biss.EmployeeManagement.Application.Commands.ChangeCustomer
{
    public class ChangeCustomerHandler : 
        IRequestHandler<ChangeCustomerRequest, ChangeCustomerResponse>
    {
        private ILogger<ChangeCustomerHandler> Logger { get; }
        private IWriteRepository<Customer> Repository { get; }        
        private IMapper Mapper { get; }

        private readonly HandleResponseExceptionHelper HandleResponseExceptionHelper;

        public ChangeCustomerHandler(
            
            IWriteRepository<Customer> repository,
            ILogger<ChangeCustomerHandler> logger,
            IMapper mapper
            )
        {
            Logger = logger;
            Repository = repository;            
            Mapper = mapper;
            HandleResponseExceptionHelper = new HandleResponseExceptionHelper(logger);
        }

        public async Task<ChangeCustomerResponse> Handle(ChangeCustomerRequest request, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Handling GetCustomerRequest");
            var response = new ChangeCustomerResponse();

            try
            {
                var entity = Mapper.Map<Customer>(request);
                entity.UpdatedAt = DateTime.Now;
                entity.UpdatedBy = "Template Api";

                var result = await Repository.Update(entity);

                if (result)
                {
                    response.Data = new ApiDataResponse<Customer>(entity);
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
