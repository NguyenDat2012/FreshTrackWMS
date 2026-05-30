using System;
using System.Collections.Generic;

namespace FreshTrackWMS.Data;

public partial class ChiTietKiemKe
{
    public int MaPhieuKiemKe { get; set; }

    public int MaLo { get; set; }

    public int TonHeThong { get; set; }

    public int ThucTeDem { get; set; }

    public string? LyDoDieuChinh { get; set; }

    public virtual LoHang MaLoNavigation { get; set; } = null!;

    public virtual PhieuKiemKe MaPhieuKiemKeNavigation { get; set; } = null!;
}
