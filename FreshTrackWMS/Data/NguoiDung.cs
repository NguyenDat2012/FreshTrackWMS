using System;
using System.Collections.Generic;

namespace FreshTrackWMS.Data;

public partial class NguoiDung
{
    public int MaNguoiDung { get; set; }

    public string TenTaiKhoan { get; set; } = null!;

    public string MatKhau { get; set; } = null!;

    public string? SoDienThoai { get; set; }

    public string? VaiTro { get; set; }

    public virtual ICollection<PhieuHuy> PhieuHuys { get; set; } = new List<PhieuHuy>();

    public virtual ICollection<PhieuKiemKe> PhieuKiemKes { get; set; } = new List<PhieuKiemKe>();

    public virtual ICollection<PhieuNhap> PhieuNhaps { get; set; } = new List<PhieuNhap>();

    public virtual ICollection<PhieuXuat> PhieuXuats { get; set; } = new List<PhieuXuat>();
}
