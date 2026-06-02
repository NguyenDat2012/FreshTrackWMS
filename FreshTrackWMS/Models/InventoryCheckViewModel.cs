using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FreshTrackWMS.ViewModels
{
    public class InventoryCheckVM
    {
        [Required]
        public DateTime NgayKiemKe { get; set; }

        public List<InventoryCheckDetailVM> Details { get; set; } = new List<InventoryCheckDetailVM>();
    }

    public class InventoryCheckDetailVM
    {
        [Required]
        public int MaLo { get; set; }

        public int TonHeThong { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng không được nhỏ hơn 0")]
        public int ThucTeDem { get; set; }

        public string? LyDoDieuChinh { get; set; }
    }
}