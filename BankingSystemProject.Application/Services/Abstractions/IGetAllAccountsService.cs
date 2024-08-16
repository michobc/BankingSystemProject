using BankingSystemProject.Application.ViewModels;

namespace BankingSystemProject.Application.Services.Abstractions;

public interface IGetAllAccountsService
{
    public Task<List<AccountViewModel>> GetAllAccounts(int userId);
}