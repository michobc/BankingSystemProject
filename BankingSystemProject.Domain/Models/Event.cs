using System;
using System.Collections.Generic;

namespace BankingSystemProject.Domain.Models;

public partial class Event
{
    public int EventId { get; set; }

    public string? BranchName { get; set; }

    public int? AccountId { get; set; }

    public int? TransactionId { get; set; }

    public string? EventType { get; set; }

    public string? EventData { get; set; }

    public DateTime? CreatedAt { get; set; }
}
