using System;
using System.Collections.Generic;

namespace FreshTrackWMS.Data;

public partial class LoHang
{
    public int MaLo { get; set; }

    public int MaThucPham { get; set; }

    public int SoLuong { get; set; }

    public DateTime NgayNhap { get; set; }

    public DateTime HanSuDung { get; set; }

    public virtual ICollection<ChiTietKiemKe> ChiTietKiemKes { get; set; } = new List<ChiTietKiemKe>();

    public virtual ICollection<ChiTietPhieuHuy> ChiTietPhieuHuys { get; set; } = new List<ChiTietPhieuHuy>();

    public virtual ICollection<ChiTietPhieuNhap> ChiTietPhieuNhaps { get; set; } = new List<ChiTietPhieuNhap>();

    public virtual ICollection<ChiTietPhieuXuat> ChiTietPhieuXuats { get; set; } = new List<ChiTietPhieuXuat>();

    public virtual ThucPham MaThucPhamNavigation { get; set; } = null!;
}
