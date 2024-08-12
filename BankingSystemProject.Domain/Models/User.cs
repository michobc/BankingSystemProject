using System;
using System.Collections.Generic;

namespace BankingSystemProject.Domain.Models;

public partial class User
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string Role { get; set; } = null!;

    public string? BranchName { get; set; }

    public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();

    public virtual Branch? BranchNameNavigation { get; set; }
}
