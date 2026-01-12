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

namespace Biss.EmployeeManagement.Application.Commands.AddCustomer
{
    public class AddCustomerHandler : IRequestHandler<AddCustomerRequest, AddCustomerResponse>
    {
        private ILogger<AddCustomerHandler> Logger { get; }
        private IWriteRepository<Customer> Repository { get; }        
        private IMapper Mapper { get; }

        private readonly HandleResponseExceptionHelper HandleResponseExceptionHelper;


        public AddCustomerHandler(
            IWriteRepository<Customer> repository,
            ILogger<AddCustomerHandler> logger,
            IMapper mapper
            )
        {
            Logger = logger;
            Repository = repository;            
            Mapper = mapper;
            HandleResponseExceptionHelper = new HandleResponseExceptionHelper(logger);
        }

        public async Task<AddCustomerResponse> Handle(AddCustomerRequest request, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Handling GetCustomerRequest");
            var response = new AddCustomerResponse();

            try
            {
                var entity = Mapper.Map<Customer>(request);
                entity.Id = Guid.NewGuid();
                entity.CreatedAt = DateTime.Now;
                entity.CreatedBy = "Template Api";
                var result = await Repository.Add(entity);

                if(result)
                {
                    response.Data = new ApiDataResponse<Customer>(entity);
                    response.Success = true;
                    response.StatusCode = 201;
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
