using FreshTrackWMS.Data;
using FreshTrackWMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FreshTrackWMS.Controllers
{
    public class SupplierController  : Controller
    {
        private readonly FreshTrackWmsContext _context; // Thay bằng tên DbContext thật của bạn
        private const int PageSize = 10; // Số lượng dòng trên mỗi trang

        public SupplierController(FreshTrackWmsContext context) => _context = context;

        // GET: Danh sách nhà cung cấp + Tìm kiếm + Phân trang
        public async Task<IActionResult> Index(string searchTerm, int page = 1)
        {
            var query = _context.NhaCungCaps.AsQueryable();

            // Xử lý tìm kiếm
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(n => n.TenNhaCungCap.Contains(searchTerm)
                                      || n.SoDienThoai.Contains(searchTerm)
                                      || n.DiaChi.Contains(searchTerm));
            }

            // Tính toán phân trang
            int totalRecords = await query.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalRecords / PageSize);
            if (page < 1) page = 1;

            var suppliers = await query
                .OrderByDescending(n => n.MaNhaCungCap)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            var viewModel = new SupplierViewModel
            {
                Suppliers = suppliers,
                SearchTerm = searchTerm,
                CurrentPage = page,
                TotalPages = totalPages,
                NewSupplier = new NhaCungCap() // Khởi tạo object trống cho Form Create
            };

            return View(viewModel);
        }

        // POST: Thêm mới Nhà cung cấp
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SupplierViewModel model)
        {
            if (ModelState.IsValid)
            {
                _context.NhaCungCaps.Add(model.NewSupplier);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            // Nếu lỗi, load lại trang Index kèm trạng thái cũ
            return RedirectToAction(nameof(Index), new { searchTerm = model.SearchTerm, page = model.CurrentPage });
        }

        // GET: API lấy dữ liệu chi tiết bằng JSON phục vụ việc Sửa (Edit) thông qua AJAX
        [HttpGet]
        public async Task<IActionResult> GetSupplierJson(int id)
        {
            var supplier = await _context.NhaCungCaps.FindAsync(id);
            if (supplier == null) return NotFound();

            return Json(new
            {
                maNcc = supplier.MaNhaCungCap,
                tenNcc = supplier.TenNhaCungCap,
                sdt = supplier.SoDienThoai,
                email = supplier.Email,
                diaChi = supplier.DiaChi
            });
        }

        // POST: Cập nhật Nhà cung cấp
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SupplierViewModel model)
        {
            if (model.NewSupplier.MaNhaCungCap > 0)
            {
                _context.NhaCungCaps.Update(model.NewSupplier);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: Xóa Nhà cung cấp
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var supplier = await _context.NhaCungCaps.FindAsync(id);
            if (supplier != null)
            {
                _context.NhaCungCaps.Remove(supplier);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
