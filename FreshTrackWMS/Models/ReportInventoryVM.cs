using System;
using System.Collections.Generic;

namespace FreshTrackWMS.ViewModels
{
    public class ReportInventoryVM
    {
        // Bộ lọc lưu lại trạng thái đã chọn trên UI
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string? SelectedCategory { get; set; }

        // Khối tổng hợp kỳ báo cáo (Hiển thị hàng ngang)
        public double TotalImport { get; set; }
        public double TotalExport { get; set; }
        public double TotalDifference => TotalImport - TotalExport;

        // Danh sách chi tiết bảng kê
        public List<ReportInventoryDetailRow> Details { get; set; } = new List<ReportInventoryDetailRow>();

        // Danh sách các nhóm danh mục thực phẩm để đổ vào thẻ Select lọc
        public List<string> Categories { get; set; } = new List<string>();
    }

    public class ReportInventoryDetailRow
    {
        public string FoodCode { get; set; } = string.Empty; // Mã TP (Ví dụ: TP001)
        public string FoodName { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty; // ĐVT
        public double BeginningQty { get; set; } // Tồn đầu kỳ
        public double InQty { get; set; }        // Nhập trong kỳ
        public double OutQty { get; set; }       // Xuất trong kỳ
        public double EndingQty => BeginningQty + InQty - OutQty; // Tồn cuối kỳ
    }
}