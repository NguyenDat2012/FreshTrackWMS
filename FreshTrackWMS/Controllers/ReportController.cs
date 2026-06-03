using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FreshTrackWMS.Data;
using FreshTrackWMS.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FreshTrackWMS.Controllers
{
    public class ReportController : Controller
    {
        private readonly FreshTrackWmsContext _context;

        public ReportController(FreshTrackWmsContext context)
        {
            _context = context;
        }

        // GET: Report/InventoryReport
        public async Task<IActionResult> Index(DateTime? fromDate, DateTime? toDate, string selectedCategory)
        {
            // Thiết lập khoảng ngày mặc định nếu người dùng chưa chọn lọc (Ví dụ: tháng hiện tại)
            DateTime start = fromDate ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DateTime end = toDate ?? DateTime.Now;

            var viewModel = new ReportInventoryVM
            {
                FromDate = start,
                ToDate = end,
                SelectedCategory = selectedCategory,
                Categories = await _context.ThucPhams
                    .Where(t => t.DanhMuc != null)
                    .Select(t => t.DanhMuc!)
                    .Distinct()
                    .ToListAsync()
            };

            // Lấy tất cả thực phẩm theo bộ lọc danh mục
            var foodsQuery = _context.ThucPhams.AsQueryable();
            if (!string.IsNullOrEmpty(selectedCategory))
            {
                foodsQuery = foodsQuery.Where(t => t.DanhMuc == selectedCategory);
            }
            var foods = await foodsQuery.ToListAsync();

            // Tính toán số liệu từng mặt hàng
            foreach (var food in foods)
            {
                // 1. Nhập trong kỳ (Tính tổng từ các chi tiết phiếu nhập nằm trong khoảng ngày)
                double inQty = await _context.ChiTietPhieuNhaps
                    .Include(ct => ct.MaPhieuNhapNavigation) // Giả định có liên kết bảng phiếu nhập chứa ngày nhập
                    .Where(ct => ct.MaLoNavigation.MaThucPham == food.MaThucPham
                              && ct.MaPhieuNhapNavigation.NgayNhap >= start
                              && ct.MaPhieuNhapNavigation.NgayNhap <= end)
                    .SumAsync(ct => (double?)ct.SoLuong) ?? 0;

                // 2. Xuất trong kỳ (Tính từ chi tiết phiếu xuất nằm trong khoảng ngày)
                double outQty = await _context.ChiTietPhieuXuats
                    .Include(ct => ct.MaPhieuXuatNavigation) // Giả định có liên kết bảng phiếu xuất chứa ngày xuất
                    .Where(ct => ct.MaLoNavigation.MaThucPham == food.MaThucPham
                              && ct.MaPhieuXuatNavigation.NgayXuat >= start
                              && ct.MaPhieuXuatNavigation.NgayXuat <= end)
                    .SumAsync(ct => (double?)ct.SoLuong) ?? 0;

                // 3. Tồn hiện tại ở các lô (Cuối kỳ)
                double endingQty = await _context.LoHangs
                    .Where(l => l.MaThucPham == food.MaThucPham)
                    .SumAsync(l => (double)l.SoLuong);

                // 4. Tồn đầu kỳ = Tồn cuối kỳ - Nhập trong kỳ + Xuất trong kỳ
                double beginningQty = endingQty - inQty + outQty;

                viewModel.Details.Add(new ReportInventoryDetailRow
                {
                    FoodCode = $"{food.MaThucPham.ToString()}",
                    FoodName = food.TenThucPham,
                    Unit = food.DonViTinh ?? "Kg",
                    BeginningQty = beginningQty,
                    InQty = inQty,
                    OutQty = outQty
                });
            }

            // Tính tổng cộng toàn bộ kỳ báo cáo
            viewModel.TotalImport = viewModel.Details.Sum(d => d.InQty);
            viewModel.TotalExport = viewModel.Details.Sum(d => d.OutQty);

            ViewData["Title"] = "Báo cáo Nhập - Xuất - Tồn";
            return View(viewModel);
        }
    }
}