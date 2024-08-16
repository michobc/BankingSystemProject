using BankingSystemProject.Application.ViewModels;
using MediatR;

namespace BankingSystemProject.Application.Commands;

public class GetAllAccounts : IRequest<List<AccountViewModel>>
{
    public string username;
}