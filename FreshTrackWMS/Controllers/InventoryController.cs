using FreshTrackWMS.Data;
using FreshTrackWMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FreshTrackWMS.Controllers
{
    public class InventoryController : Controller
    {
        private readonly FreshTrackWmsContext _context;
        private const int PageSize = 10;

        public InventoryController(FreshTrackWmsContext context) => _context = context;

        // GET: Inventory
        public async Task<IActionResult> Index(string searchTerm, int? warningFilter, string categoryFilter, int page = 1)
        {
            // 1. Khởi tạo truy vấn từ bảng ThucPhams kèm theo các LoHangs của nó
            var query = _context.ThucPhams
                                .Include(t => t.LoHangs)
                                .AsQueryable();

            // 2. Lọc theo từ khóa tìm kiếm (Mã số hoặc Tên thực phẩm)
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.Trim();
                query = query.Where(t => t.TenThucPham.Contains(searchTerm)
                                      || t.MaThucPham.ToString() == searchTerm);
            }

            // 3. Lọc theo Danh mục
            if (!string.IsNullOrEmpty(categoryFilter))
            {
                query = query.Where(t => t.DanhMuc == categoryFilter);
            }

            // 4. Lấy toàn bộ danh sách đã lọc sơ bộ về bộ nhớ để xử lý logic trạng thái phức tạp
            var rawData = await query.ToListAsync();

            // 5. Khởi tạo mảng trung gian để mapping sang đối tượng hiển thị (InventoryItem)
            var mappedItems = rawData.Select(t => {

                // Giả định trong entity LoHang của bạn có các trường tương ứng: 
                // MaLo, NgayNhap, MaPhieuNhap, SoLuongTon, NgayHetHan, TrangThai
                // (Nếu tên trường trong LoHang của bạn khác, hãy đổi lại cho đúng ở đây)
                var batches = t.LoHangs.Select(l => new BatchItem
                {
                    BatchId = l.GetType().GetProperty("MaLo")?.GetValue(l)?.ToString() ?? "N/A",
                    ImportDate = Convert.ToDateTime(l.GetType().GetProperty("NgayNhap")?.GetValue(l) ?? DateTime.Now),
                    ImportCode = l.GetType().GetProperty("MaPhieuNhap")?.GetValue(l)?.ToString() ?? "N/A",
                    BatchStock = Convert.ToDouble(l.GetType().GetProperty("SoLuongTon")?.GetValue(l) ?? 0),
                    ExpiryDate = Convert.ToDateTime(l.GetType().GetProperty("NgayHetHan")?.GetValue(l) ?? DateTime.Now),
                    Status = l.GetType().GetProperty("TrangThai")?.GetValue(l)?.ToString() ?? "Tốt"
                }).ToList();

                // Tính toán các trường hiển thị tổng hợp
                double totalStock = batches.Sum(b => b.BatchStock);

                // Logic xác định trạng thái (StatusType):
                // Giả sử có lô nào hạn sử dụng còn dưới 7 ngày so với hiện tại (2026) -> Sắp hết hạn (1)
                // Nếu tổng tồn kho dưới 10 -> Sắp hết hàng (2), Ngược lại -> Bình thường (0)
                int statusType = 0;
                if (totalStock <= 10 && totalStock > 0)
                {
                    statusType = 2; // Sắp hết hàng
                }
                else if (batches.Any(b => (b.ExpiryDate - DateTime.Now).TotalDays <= 7 && b.BatchStock > 0))
                {
                    statusType = 1; // Có lô sắp hết hạn
                }

                return new InventoryItem
                {
                    ItemCode = "TP" + t.MaThucPham.ToString("D3"), // Định dạng hiển thị thành TP001, TP002...
                    ItemName = t.TenThucPham,
                    Category = t.DanhMuc ?? "Chưa phân loại",
                    Unit = t.DonViTinh ?? "Kg",
                    TotalStock = totalStock,
                    StatusType = statusType,
                    Batches = batches
                };
            }).AsQueryable();

            // 6. Thực hiện Lọc theo loại cảnh báo (sau khi đã tính toán xong StatusType)
            if (warningFilter.HasValue && warningFilter.Value > 1)
            {
                int targetStatus = warningFilter.Value == 2 ? 1 : 2;
                mappedItems = mappedItems.Where(i => i.StatusType == targetStatus);
            }

            // 7. Xử lý phân trang trên tập dữ liệu đã map
            int totalRecords = mappedItems.Count();
            int totalPages = (int)Math.Ceiling((double)totalRecords / PageSize);
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            var pagedItems = mappedItems
                .OrderBy(i => i.ItemCode)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            // 8. Đóng gói gửi ra View
            var viewModel = new InventoryPageViewModel
            {
                Items = pagedItems,
                SearchTerm = searchTerm,
                WarningFilter = warningFilter,
                CategoryFilter = categoryFilter,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalRecords = totalRecords
            };

            return View(viewModel);
        }
       
    }
}