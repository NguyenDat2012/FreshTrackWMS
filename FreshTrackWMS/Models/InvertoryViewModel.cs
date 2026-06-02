using System;
using System.Collections.Generic;

namespace FreshTrackWMS.Models
{
    public class BatchItem
    {
        public string BatchId { get; set; }      // Mã lô (MaLo)
        public DateTime ImportDate { get; set; }  // Ngày nhập (NgayNhap)
        public double BatchStock { get; set; }    // Số lượng của lô này (SoLuong)
        public DateTime ExpiryDate { get; set; }  // Hạn sử dụng (HanSuDung)
    }

    public class InventoryItem
    {
        public string ItemCode { get; set; }      // Định dạng kiểu TP001
        public string ItemName { get; set; }
        public string Category { get; set; }
        public string Unit { get; set; }
        public double TotalStock { get; set; }    // TỔNG SỐ LƯỢNG TỒN GOM TỪ CÁC LÔ
        public int StatusType { get; set; }       // 0: Bình thường, 1: Sắp hết hạn, 2: Sắp hết hàng
        public List<BatchItem> Batches { get; set; }
    }

    public class InventoryPageViewModel
    {
        public List<InventoryItem> Items { get; set; }
        public string SearchTerm { get; set; }
        public int? WarningFilter { get; set; }
        public string CategoryFilter { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }
    }
}