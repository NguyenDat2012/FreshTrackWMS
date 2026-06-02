using System.Collections.Generic;
using FreshTrackWMS.Models; // Hoặc namespace chứa thực thể ChiTietPhieuHuy của bạn

namespace FreshTrackWMS.Models
{
    public class CreateDestroyTicketInputModel
    {
        
        public string TenNguoiTao { get; set; } = string.Empty;

        // Nhận danh sách các dòng thực phẩm cần hủy (details[0], details[1]...)
        public List<ChiTietPhieuHuyInput> Details { get; set; } = new List<ChiTietPhieuHuyInput>();
    }

    // Class phụ để hứng chính xác dữ liệu từ các cột trong bảng của Form
    public class ChiTietPhieuHuyInput
    {
        public int MaLo { get; set; }
        public int SoLuong { get; set; }
        public string LyDo { get; set; } = string.Empty;
    }
}