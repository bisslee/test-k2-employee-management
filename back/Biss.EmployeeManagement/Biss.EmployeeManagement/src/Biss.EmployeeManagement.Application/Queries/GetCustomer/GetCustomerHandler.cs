using AutoMapper;
using MediatR;
using Biss.EmployeeManagement.Application.Helpers;
using Biss.EmployeeManagement.Domain.Constants;
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

namespace Biss.EmployeeManagement.Application.Queries.GetCustomer
{
    public class GetCustomerHandler : IRequestHandler<GetCustomerRequest, GetCustomerResponse>
    {
        private ILogger<GetCustomerHandler> Logger { get; }
        private IMapper Mapper { get; }
        private IReadRepository<Customer> Repository { get; }

        private readonly HandleResponseExceptionHelper HandleResponseExceptionHelper;

        public GetCustomerHandler(

            IReadRepository<Customer> repository,
            IMapper mapper,
            ILogger<GetCustomerHandler> logger
            )
        {
            Logger = logger;
            Mapper = mapper;
            Repository = repository;
            HandleResponseExceptionHelper = new HandleResponseExceptionHelper(logger);
        }

        public async Task<GetCustomerResponse> Handle(
            GetCustomerRequest request, 
            CancellationToken cancellationToken)
        {
            Logger.LogInformation("Handling GetCustomerRequest");
            var response = new GetCustomerResponse();
            try
            {

                request = request.LoadPagination();

                Expression<Func<Customer, bool>> predicate = customer =>
                (string.IsNullOrEmpty(request.FullName) || customer.FullName.Contains(request.FullName))
                && (string.IsNullOrEmpty(request.Email) || customer.Email.Contains(request.Email))
                && (string.IsNullOrEmpty(request.DocumentNumber) || customer.DocumentNumber.Contains(request.DocumentNumber))
                && (string.IsNullOrEmpty(request.Phone) || customer.Phone.Contains(request.Phone))              
                && ((request.StartBirthDate == null || customer.BirthDate >= request.StartBirthDate)
                    && (request.EndBirthDate == null || customer.BirthDate <= request.EndBirthDate))
                && (request.Active == null || customer.Active == request.Active);

                if (request.FieldName == null)
                {
                    request.FieldName = "FullName";
                }

                var (customers, totalItems) = await Repository.FindWithPagination
                    (
                        predicate,
                        request.Page,
                        request.Offset,
                        request.FieldName,
                        request.Order
                    );


                response.Metadata = new ApiMetaDataResponse
                    (
                        totalItems, 
                        request.Offset, 
                        request.Page
                    );

                if (customers != null &&  customers.Any())
                {
                    response.Data = new ApiDataResponse<List<Customer>>(customers);
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
