using System;
using System.Collections.Generic;

namespace FreshTrackWMS.Data;

public partial class PhieuKiemKe
{
    public int MaPhieuKiemKe { get; set; }

    public DateTime NgayKiemKe { get; set; }

    public int NguoiTao { get; set; }

    public virtual ICollection<ChiTietKiemKe> ChiTietKiemKes { get; set; } = new List<ChiTietKiemKe>();

    public virtual NguoiDung NguoiTaoNavigation { get; set; } = null!;
}
