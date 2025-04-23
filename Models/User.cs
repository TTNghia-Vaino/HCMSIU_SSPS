using System;
using System.Collections.Generic;

namespace HCMSIU_SSPS.Models;

public partial class User
{
    public int UserId { get; set; }

    public string? FullName { get; set; }

    public string UserName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public int Role { get; set; }

    public int? PageBalance { get; set; }

    public virtual ICollection<PrintJob> PrintJobs { get; set; } = new List<PrintJob>();

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
