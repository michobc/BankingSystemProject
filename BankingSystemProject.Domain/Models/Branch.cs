using System;
using System.Collections.Generic;

namespace BankingSystemProject.Domain.Models;

public partial class Branch
{
    public string BranchName { get; set; } = null!;

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
