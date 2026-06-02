using System;
using System.Collections.Generic;

namespace FreshTrackWMS.Models
{
    public class ExportTicketItemViewModel
    {
        public int MaPhieuXuat { get; set; }
        public DateTime NgayXuat { get; set; }
        public string LyDoXuat { get; set; }
        public int SoMatHang { get; set; }
        public int TongSoLuong { get; set; }
        public string TenNguoiTao { get; set; }
    }

    public class CreateExportInputModel
    {
        public DateTime NgayXuat { get; set; }
        public string TenNguoiTao { get; set; }
        public string LyDoXuat { get; set; }
        public string GhiChuTongQuan { get; set; }
        public List<ExportDetailInput> Details { get; set; } = new List<ExportDetailInput>();
    }

    public class ExportDetailInput
    {
        public int MaLo { get; set; }
        public int SoLuongXuat { get; set; }
        public string GhiChuChiTiet { get; set; }
    }

    public class ExportPageViewModel
    {
        public List<ExportTicketItemViewModel> Tickets { get; set; } = new List<ExportTicketItemViewModel>();
        public string SearchTerm { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}