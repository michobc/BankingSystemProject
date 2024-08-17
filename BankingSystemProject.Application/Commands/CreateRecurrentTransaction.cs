using BankingSystemProject.Application.ViewModels;
using MediatR;

namespace BankingSystemProject.Application.Commands;

public class CreateRecurrentTransaction : IRequest<RecurrenttransactionViewModel>
{
    public string BranchId { get; set; }
    public string CustomerUsername { get; set; }
    public int AccountId { get; set; }
    public decimal Amount { get; set; }
    public string TransactionType { get; set; } // "Withdrawal" or "Deposit"
    public string Frequency { get; set; } = null!;
}