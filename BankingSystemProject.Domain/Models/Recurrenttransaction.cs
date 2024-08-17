namespace BankingSystemProject.Domain.Models;

public partial class Recurrenttransaction
{
    public int Recurrenttransactionid { get; set; }

    public int Accountid { get; set; }

    public decimal Amount { get; set; }

    public string Transactiontype { get; set; } = null!;

    public DateTime? Createdat { get; set; }

    public string Frequency { get; set; } = null!;

    public DateTime Nexttransactiondate { get; set; }

    public virtual Account Account { get; set; } = null!;
}
