using BankingSystemProject.Application.ViewModels;
using MediatR;

namespace BankingSystemProject.Application.Commands;

public class GetTransactions: IRequest<List<TransactionViewModel>>
{
    public string Branch { get; set; }
    public int AccountId { get; set; }
}