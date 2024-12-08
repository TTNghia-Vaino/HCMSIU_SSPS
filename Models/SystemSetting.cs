using System;
using System.Collections.Generic;

namespace HCMSIU_SSPS.Models;

public partial class SystemSetting
{
    public int SettingId { get; set; }

    public string SettingKey { get; set; } = null!;

    public string SettingValue { get; set; } = null!;

    public DateTime? LastUpdated { get; set; }
}
