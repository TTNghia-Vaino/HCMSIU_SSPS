using System;
using System.Collections.Generic;

namespace HCMSIU_SSPS.Models;

public partial class Printer
{
    public int PrinterId { get; set; }

    public string PrinterName { get; set; } = null!;

    public string? Brand { get; set; }

    public string? Model { get; set; }

    public string? Location { get; set; }

    public virtual ICollection<PrintJob> PrintJobs { get; set; } = new List<PrintJob>();
}
