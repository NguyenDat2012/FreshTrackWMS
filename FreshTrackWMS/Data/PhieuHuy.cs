using System;
using System.Collections.Generic;

namespace FreshTrackWMS.Data;

public partial class PhieuHuy
{
    public int MaPhieuHuy { get; set; }

    public DateTime NgayHuy { get; set; }

    public int NguoiTao { get; set; }

    public virtual ICollection<ChiTietPhieuHuy> ChiTietPhieuHuys { get; set; } = new List<ChiTietPhieuHuy>();

    public virtual NguoiDung NguoiTaoNavigation { get; set; } = null!;
}
