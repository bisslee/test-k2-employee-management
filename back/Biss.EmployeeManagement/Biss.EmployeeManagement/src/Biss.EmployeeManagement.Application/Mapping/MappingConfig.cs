using AutoMapper;
using MediatR;
using Biss.EmployeeManagement.Application.Commands.AddCustomer;
using Biss.EmployeeManagement.Application.Commands.ChangeCustomer;
using Biss.EmployeeManagement.Application.Queries.GetCustomer;
using Biss.EmployeeManagement.Domain.Entities;
using System;

namespace Biss.EmployeeManagement.Application.Mapping
{
    public class MappingConfig : Profile
    {
        public MappingConfig()
        {
            CreateMap<Customer, GetCustomerResponse>().ReverseMap();
            CreateMap<Func<Customer, bool>, GetCustomerRequest >().ReverseMap();
            CreateMap<Customer, AddCustomerResponse>().ReverseMap();
            CreateMap<Customer, AddCustomerRequest>().ReverseMap();
            CreateMap<Customer, ChangeCustomerRequest>().ReverseMap();
        }
    }
}
