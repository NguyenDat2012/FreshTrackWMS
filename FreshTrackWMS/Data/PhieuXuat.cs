using System;
using System.Collections.Generic;

namespace FreshTrackWMS.Data;

public partial class PhieuXuat
{
    public int MaPhieuXuat { get; set; }

    public DateTime NgayXuat { get; set; }

    public int NguoiTao { get; set; }

    public string? LyDoXuat { get; set; }

    public virtual ICollection<ChiTietPhieuXuat> ChiTietPhieuXuats { get; set; } = new List<ChiTietPhieuXuat>();

    public virtual NguoiDung NguoiTaoNavigation { get; set; } = null!;
}
