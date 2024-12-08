using System;
using System.Collections.Generic;

namespace HCMSIU_SSPS.Models;

public partial class PrintJob
{
    public int PrintJobId { get; set; }

    public int? UserId { get; set; }

    public int? PrinterId { get; set; }

    public string? FileName { get; set; }

    public int? PageCount { get; set; }

    public int? TotalPages { get; set; }

    public int? Copies { get; set; }

    public bool? IsDoubleSided { get; set; }

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public virtual Printer? Printer { get; set; }

    public virtual User? User { get; set; }
}
