using BankingSystemProject.Application.ViewModels;
using MediatR;

namespace BankingSystemProject.Application.Commands;

public class GetAllMyAccounts: IRequest<List<AccountViewModel>>
{
}