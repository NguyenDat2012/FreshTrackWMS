using System;
using System.Collections.Generic;

namespace FreshTrackWMS.Data;

public partial class ChiTietPhieuHuy
{
    public int MaPhieuHuy { get; set; }

    public int MaLo { get; set; }

    public int SoLuong { get; set; }

    public string? LyDo { get; set; }

    public virtual LoHang MaLoNavigation { get; set; } = null!;

    public virtual PhieuHuy MaPhieuHuyNavigation { get; set; } = null!;
}
