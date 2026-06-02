using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FreshTrackWMS.ViewModels
{
    public class ImportTicketVM
    {
        [Required]
        public DateTime NgayNhap { get; set; }

        [Required]
        public int MaNhaCungCap { get; set; }

        public string? GhiChu { get; set; }

        // Danh sách các mặt hàng nhập kho
        public List<ImportTicketDetailVM> Details { get; set; } = new List<ImportTicketDetailVM>();
    }

    public class ImportTicketDetailVM
    {
        [Required]
        public int MaThucPham { get; set; } // Hứng từ UI

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int SoLuong { get; set; }


        [Required]
        public DateTime HanSuDung { get; set; }
    }
}