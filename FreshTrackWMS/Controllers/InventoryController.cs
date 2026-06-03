using FreshTrackWMS.Data;
using FreshTrackWMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FreshTrackWMS.Controllers
{
    public class InventoryController : Controller
    {
        private readonly FreshTrackWmsContext _context;
        private const int PageSize = 10; // Đã loại bỏ dòng khai báo trùng lặp bên dưới

        public InventoryController(FreshTrackWmsContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchTerm, int? warningFilter, string categoryFilter, int page = 1)
        {
            // 1. Tạo truy vấn gốc và nạp kèm danh sách các lô hàng để tính tổng
            var query = _context.ThucPhams
                                .Include(t => t.LoHangs)
                                .AsQueryable();

            // 2. Bộ lọc tìm kiếm theo Tên hoặc Mã số thực phẩm
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.Trim();
                query = query.Where(t => t.TenThucPham.Contains(searchTerm)
                                      || t.MaThucPham.ToString() == searchTerm);
            }

            // 3. Bộ lọc Danh mục sản phẩm
            if (!string.IsNullOrEmpty(categoryFilter))
            {
                query = query.Where(t => t.DanhMuc == categoryFilter);
            }

            // 4. Lấy dữ liệu thô từ Database về RAM để xử lý toán tử so sánh ngày tháng phức tạp
            var rawData = await query.ToListAsync();

            // 5. Duyệt danh sách thực phẩm, bốc các lô hàng và tính toán tổng tồn kho
            var mappedItems = rawData.Select(t => {

                // Đọc toàn bộ danh sách các lô thuộc thực phẩm này
                var batches = t.LoHangs.Select(l => new BatchItem
                {
                    BatchId = l.MaLo.ToString(), // Ép kiểu int từ DB sang string của ViewModel an toàn
                    ImportDate = l.NgayNhap,
                    BatchStock = Convert.ToDouble(l.SoLuong),
                    ExpiryDate = l.HanSuDung
                }).ToList();

                // Tính tổng lượng tồn kho đồng nhất bằng cách cộng dồn tất cả các mã lô
                double totalStock = batches.Sum(b => b.BatchStock);

                // Mặc định: 0 - Bình thường
                int statusType = 0;

                // Tối ưu logic: Kiểm tra trạng thái số lượng trước
                if (totalStock <= 0)
                {
                    statusType = 3; // Hết sạch hàng trong kho (bổ sung để giao diện hiển thị rõ ràng)
                }
                else if (totalStock <= 10)
                {
                    statusType = 2; // Sắp hết hàng (Tổng số lượng tồn nhỏ hơn hoặc bằng 10)
                }
                // Nếu hàng còn nhiều, mới xét xem có lô nào đang bị cận ngày hết hạn hay không (trong vòng 7 ngày)
                else if (batches.Any(b => b.BatchStock > 0 && (b.ExpiryDate - DateTime.Now).TotalDays <= 7))
                {
                    statusType = 1; // Có lô hàng sắp hết hạn
                }

                return new InventoryItem
                {
                    ItemCode = t.MaThucPham.ToString(), 
                    ItemName = t.TenThucPham,
                    Category = t.DanhMuc ?? "Chưa phân loại",
                    Unit = t.DonViTinh ?? "Kg",
                    TotalStock = totalStock,
                    StatusType = statusType,
                    Batches = batches
                };
            }).ToList();

            // 6. Lọc theo bộ lọc cảnh báo trên giao diện (warningFilter)
            if (warningFilter.HasValue && warningFilter.Value > 0)
            {
                mappedItems = mappedItems.Where(i => i.StatusType == warningFilter.Value).ToList();
            }

            // 7. Xử lý phân trang khoa học trên tập dữ liệu đã tổng hợp
            int totalRecords = mappedItems.Count;
            int totalPages = (int)Math.Ceiling((double)totalRecords / PageSize);

            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            var pagedItems = mappedItems
                .OrderBy(i => i.ItemCode)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            // 8. Đóng gói dữ liệu hoàn chỉnh đẩy ra ngoài View
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