using BankingSystemProject.Application.ViewModels;
using MediatR;

namespace BankingSystemProject.Application.Commands;

public class CreateTransaction : IRequest<TransactionViewModel>
{
    public string BranchId { get; set; }
    public int AccountId { get; set; }
    public decimal Amount { get; set; }
    public string TransactionType { get; set; } // "Withdrawal" or "Deposit"
}