using System;
using System.Collections.Generic;

namespace FreshTrackWMS.Models
{
    public class InventoryItem
    {
        public string ItemCode { get; set; }      // Sẽ nhận giá trị MaThucPham.ToString()
        public string ItemName { get; set; }      // Nhận TenThucPham
        public string Category { get; set; }      // Nhận DanhMuc
        public double TotalStock { get; set; }    // Tính tổng số lượng từ các LoHangs
        public string Unit { get; set; }          // Nhận DonViTinh
        public int StatusType { get; set; }       // Trạng thái tính toán dựa trên HSD của các lô
        public List<BatchItem> Batches { get; set; } = new List<BatchItem>();
    }

    public class BatchItem
    {
        public string BatchId { get; set; }       // Mã lô hàng
        public DateTime ImportDate { get; set; }  // Ngày nhập
        public string ImportCode { get; set; }    // Mã phiếu nhập
        public double BatchStock { get; set; }    // Số lượng tồn lô
        public DateTime ExpiryDate { get; set; }  // Hạn sử dụng
        public string Status { get; set; }        // Trạng thái lô
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