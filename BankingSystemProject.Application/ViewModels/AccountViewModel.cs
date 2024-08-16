namespace BankingSystemProject.Application.ViewModels;

public class AccountViewModel
{
    public int AccountId { get; set; }

    public int UserId { get; set; }

    public decimal Balance { get; set; }

    public DateTime? CreatedAt { get; set; }
    
    public string BranchId { get; set; }
}