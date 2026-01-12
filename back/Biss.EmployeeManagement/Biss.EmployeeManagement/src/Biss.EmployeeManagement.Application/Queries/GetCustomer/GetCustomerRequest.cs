using MediatR;
using System;

namespace Biss.EmployeeManagement.Application.Queries.GetCustomer
{
    public class GetCustomerRequest : BaseRequest, IRequest<GetCustomerResponse>
    {
        
        public DateTime? StartBirthDate { get; set; }
        public DateTime? EndBirthDate { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? DocumentNumber { get; set; }
        public string? Gender { get; set; }
        public string? Phone { get; set; }
        public string? Password { get; set; }
        public bool? Active { get; set; }

    }
}
