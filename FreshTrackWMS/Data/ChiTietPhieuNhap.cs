using System;
using System.Collections.Generic;

namespace FreshTrackWMS.Data;

public partial class ChiTietPhieuNhap
{
    public int MaPhieuNhap { get; set; }

    public int MaLo { get; set; }

    public int SoLuong { get; set; }

    public DateTime NgaySanXuat { get; set; }

    public DateTime HanSuDung { get; set; }

    public virtual LoHang MaLoNavigation { get; set; } = null!;

    public virtual PhieuNhap MaPhieuNhapNavigation { get; set; } = null!;
}
