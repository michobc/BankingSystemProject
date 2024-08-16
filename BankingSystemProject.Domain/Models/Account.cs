using System;
using System.Collections.Generic;

namespace BankingSystemProject.Domain.Models;

public partial class Account
{
    public int AccountId { get; set; }

    public int UserId { get; set; }

    public decimal Balance { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    public virtual User User { get; set; } = null!;
}
