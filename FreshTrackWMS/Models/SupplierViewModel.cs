using FreshTrackWMS.Data;

namespace FreshTrackWMS.Models
{
    public class SupplierViewModel
    {
        // Danh sách hiển thị lên bảng dữ liệu
        public List<NhaCungCap> Suppliers { get; set; } = new List<NhaCungCap>();

        // Phục vụ tìm kiếm và phân trang
        public string SearchTerm { get; set; } = string.Empty;
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;

        // Đối tượng dùng riêng cho Form Thêm/Sửa trong Modal
        public NhaCungCap NewSupplier { get; set; } = new NhaCungCap();
    }
}