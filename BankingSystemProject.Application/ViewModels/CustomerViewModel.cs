namespace BankingSystemProject.Application.ViewModels;

public class CustomerViewModel
{
    public string? username { get; set; }
    public string? Branch { get; set; }
    public ICollection<AccountViewModel> Accounts { get; set; }
}