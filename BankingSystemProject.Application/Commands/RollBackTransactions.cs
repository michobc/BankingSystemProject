using BankingSystemProject.Application.ViewModels;
using MediatR;

namespace BankingSystemProject.Application.Commands;

public class RollBackTransactions : IRequest<List<TransactionViewModel>>
{
    public int AccountId { get; set; }
    public string BranchId { get; set; }
    public DateTime Date { get; set; }
}