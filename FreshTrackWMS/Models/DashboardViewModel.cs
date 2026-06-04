namespace FreshTrackWMS.Models
{
    public class DashboardViewModel
    {
        public int SapHetHangCount { get; set; }
        public int SapHetHanCount { get; set; }
        public int PhieuNhapHomNayCount { get; set; }
        public int TongTonKho { get; set; }
        public List<RecentImportTicketVM> PhieuNhapGanDay { get; set; } = new List<RecentImportTicketVM>();
    }

    public class RecentImportTicketVM
    {
        public int MaPhieuNhap { get; set; }
        public DateTime NgayNhap { get; set; }
        public string? TenNhaCungCap { get; set; }
        public int SoMatHang { get; set; }
    }
}

