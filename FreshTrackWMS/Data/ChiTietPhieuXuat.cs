using System;
using System.Collections.Generic;

namespace FreshTrackWMS.Data;

public partial class ChiTietPhieuXuat
{
    public int MaPhieuXuat { get; set; }

    public int MaLo { get; set; }

    public int SoLuong { get; set; }

    public string? GhiChu { get; set; }

    public virtual LoHang MaLoNavigation { get; set; } = null!;

    public virtual PhieuXuat MaPhieuXuatNavigation { get; set; } = null!;
}
