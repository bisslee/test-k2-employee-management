using MediatR;
using System;

namespace Biss.EmployeeManagement.Application.Queries.Employees.GetEmployeeByKey
{
    public class GetEmployeeByKeyRequest : IRequest<GetEmployeeByKeyResponse>
    {
        public Guid Id { get; set; }
    }
}
