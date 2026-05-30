using System;
using System.Collections.Generic;

namespace FreshTrackWMS.Data;

public partial class PhieuNhap
{
    public int MaPhieuNhap { get; set; }

    public DateTime NgayNhap { get; set; }

    public int MaNhaCungCap { get; set; }

    public int NguoiTao { get; set; }

    public string? GhiChu { get; set; }

    public virtual ICollection<ChiTietPhieuNhap> ChiTietPhieuNhaps { get; set; } = new List<ChiTietPhieuNhap>();

    public virtual NhaCungCap MaNhaCungCapNavigation { get; set; } = null!;

    public virtual NguoiDung NguoiTaoNavigation { get; set; } = null!;
}
