using Microsoft.AspNetCore.Mvc.Rendering;

namespace FreshTrackWMS.Models
{
    public class XuatKhoIndexViewModel
    {
        public List<XuatKhoHistoryItem> PhieuXuats { get; set; } = new();

        public List<XuatKhoTonKhoItem> TonKhoItems { get; set; } = new();

        public List<SelectListItem> NguoiTaoOptions { get; set; } = new();

        public CreateXuatKhoRequest NewPhieuXuat { get; set; } = new();
    }

    public class XuatKhoHistoryItem
    {
        public int MaPhieuXuat { get; set; }

        public string MaPhieu { get; set; } = string.Empty;

        public DateTime NgayXuat { get; set; }

        public string LyDoXuat { get; set; } = string.Empty;

        public int SoDonHang { get; set; }

        public int TongSoLuong { get; set; }

        public string NguoiTao { get; set; } = string.Empty;
    }

    public class XuatKhoTonKhoItem
    {
        public int MaLo { get; set; }

        public int MaThucPham { get; set; }

        public string TenThucPham { get; set; } = string.Empty;

        public string DonViTinh { get; set; } = string.Empty;

        public int SoLuongTon { get; set; }

        public DateTime HanSuDung { get; set; }

        public string DisplayName => $"TP{MaThucPham:D3} - {TenThucPham}";

        public string LoDisplayName => $"Lô Hàng-{MaLo:D3} (HSD: {HanSuDung:dd/MM/yyyy}) - Gợi ý Xuất Trước";
    }

    public class CreateXuatKhoRequest
    {
        public DateTime NgayXuat { get; set; } = DateTime.Today;

        public string LyDoXuat { get; set; } = string.Empty;

        public int NguoiTao { get; set; }

        public int SoDonHang { get; set; }

        public string GhiChu { get; set; } = string.Empty;

        public List<CreateXuatKhoDetailRequest> Details { get; set; } = new();
    }

    public class CreateXuatKhoDetailRequest
    {
        public int MaLo { get; set; }

        public int SoLuong { get; set; }

        public string GhiChu { get; set; } = string.Empty;
    }
}
