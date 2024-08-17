namespace BankingSystemProject.Application.ViewModels;

public class TransactionViewModel
{
    public int TransactionId { get; set; }

    public int AccountId { get; set; }

    public decimal Amount { get; set; }

    public string TransactionType { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }
}