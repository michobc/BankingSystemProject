using AutoMapper;
using BankingSystemProject.Application.ViewModels;
using BankingSystemProject.Domain.Models;

namespace BankingSystemProject.Application.Mappings;

public class BankingSystemMappings : Profile
{
    public BankingSystemMappings()
    {
        CreateMap<User, EmployeeViewModel>();
        CreateMap<User, CustomerViewModel>();
    }
}