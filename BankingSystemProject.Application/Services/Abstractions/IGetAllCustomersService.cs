using BankingSystemProject.Application.ViewModels;

namespace BankingSystemProject.Application.Services.Abstractions;

public interface IGetAllCustomersService
{
    public Task<List<CustomerViewModel>> GetAllCustomers(string branch);
}