using System;
using System.Collections.Generic;

namespace HCMSIU_SSPS.Models;

public partial class Transaction
{
    public int TransactionId { get; set; }

    public int? UserId { get; set; }

    public decimal? Amount { get; set; }

    public int? Status { get; set; }

    public string? Description { get; set; }

    public DateTime? Timestamp { get; set; }

    public virtual User? User { get; set; }
}
