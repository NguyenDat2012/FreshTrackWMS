using System;
using System.Collections.Generic;

namespace FreshTrackWMS.Data;

public partial class ThucPham
{
    public int MaThucPham { get; set; }

    public string TenThucPham { get; set; } = null!;

    public string? DanhMuc { get; set; }

    public string? DonViTinh { get; set; }

    public string? PhuongThucBaoQuan { get; set; }

    public int? HanSuDungThamKhao { get; set; }

    public virtual ICollection<LoHang> LoHangs { get; set; } = new List<LoHang>();
}
