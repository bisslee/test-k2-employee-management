using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biss.EmployeeManagement.Application.Queries.GetCustomerByKey
{
    public class GetCustomerByKeyRequest : IRequest<GetCustomerByKeyResponse>
    {
        public Guid Id { get; set; }
    }
}
