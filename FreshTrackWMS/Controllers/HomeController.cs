using FreshTrackWMS.Data;
using FreshTrackWMS.Models;
using FreshTrackWMS.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FreshTrackWMS.Controllers
{
    public class HomeController : Controller
    {
        private readonly FreshTrackWmsContext _context;

        public HomeController(FreshTrackWmsContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;
            var nextWeek = today.AddDays(7);

            // 1. Đếm sản phẩm sắp hết hàng (Tổng tồn < 10)
            var sapHetHang = await _context.ThucPhams
                .Where(t => t.LoHangs.Sum(l => l.SoLuong) < 10)
                .CountAsync();

            // 2. Đếm lô hàng sắp hết hạn (trong 7 ngày tới và còn tồn kho)
            var sapHetHan = await _context.LoHangs
                .Where(l => l.SoLuong > 0 && l.HanSuDung >= today && l.HanSuDung <= nextWeek)
                .CountAsync();

            // 3. Đếm phiếu nhập trong ngày hôm nay
            var phieuNhapHomNay = await _context.PhieuNhaps
                .Where(p => p.NgayNhap.Date == today)
                .CountAsync();

            // 4. Tính tổng tồn kho tất cả các mặt hàng
            var tongTonKho = await _context.LoHangs.SumAsync(l => l.SoLuong);

            // 5. Lấy 5 phiếu nhập gần nhất
            var phieuNhapGanDay = await _context.PhieuNhaps
                .Include(p => p.MaNhaCungCapNavigation)
                .Include(p => p.ChiTietPhieuNhaps)
                .OrderByDescending(p => p.NgayNhap)
                .ThenByDescending(p => p.MaPhieuNhap)
                .Take(5)
                .Select(p => new RecentImportTicketVM
                {
                    MaPhieuNhap = p.MaPhieuNhap,
                    NgayNhap = p.NgayNhap,
                    TenNhaCungCap = p.MaNhaCungCapNavigation != null ? p.MaNhaCungCapNavigation.TenNhaCungCap : "N/A",
                    SoMatHang = p.ChiTietPhieuNhaps.Count
                }).ToListAsync();

            // Gom tất cả vào ViewModel
            var dashboardData = new DashboardViewModel
            {
                SapHetHangCount = sapHetHang,
                SapHetHanCount = sapHetHan,
                PhieuNhapHomNayCount = phieuNhapHomNay,
                TongTonKho = tongTonKho,
                PhieuNhapGanDay = phieuNhapGanDay
            };

            return View(dashboardData);
        }

        // --- CÁC API DÙNG CHO AJAX MODAL TRÊN DASHBOARD ---

        [HttpGet]
        public async Task<IActionResult> GetLowStockDetails()
        {
            var data = await _context.ThucPhams
                .Where(t => t.LoHangs.Sum(l => l.SoLuong) < 10)
                .Select(t => new {
                    ma = "TP" + t.MaThucPham.ToString("D3"),
                    ten = t.TenThucPham,
                    ton = t.LoHangs.Sum(l => l.SoLuong)
                }).ToListAsync();
            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetExpiringDetails()
        {
            var today = DateTime.Today;
            var nextWeek = today.AddDays(7);
            var data = await _context.LoHangs
                .Where(l => l.SoLuong > 0 && l.HanSuDung >= today && l.HanSuDung <= nextWeek)
                .Select(l => new {
                    maLo = "Lô " + l.MaLo.ToString("D3"),
                    ten = l.MaThucPhamNavigation.TenThucPham,
                    hsd = l.HanSuDung.ToString("dd/MM/yyyy"),
                    sl = l.SoLuong
                }).ToListAsync();
            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetTodayImportDetails()
        {
            var today = DateTime.Today;
            var data = await _context.PhieuNhaps
                .Where(p => p.NgayNhap.Date == today)
                .Select(p => new {
                    maPhieu = "PN-" + p.NgayNhap.ToString("yyMMdd") + "-" + p.MaPhieuNhap.ToString("D2"),
                    ncc = p.MaNhaCungCapNavigation != null ? p.MaNhaCungCapNavigation.TenNhaCungCap : "N/A",
                    slMatHang = p.ChiTietPhieuNhaps.Count
                }).ToListAsync();
            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetInventoryDetails()
        {
            // Lấy danh sách tồn kho (gom nhóm theo thực phẩm)
            var data = await _context.ThucPhams
                .Where(t => t.LoHangs.Any(l => l.SoLuong > 0))
                .Select(t => new {
                    ma = "TP" + t.MaThucPham.ToString("D3"),
                    ten = t.TenThucPham,
                    ton = t.LoHangs.Sum(l => l.SoLuong)
                })
                .OrderByDescending(x => x.ton) // Ưu tiên xếp thằng tồn nhiều lên đầu
                .ToListAsync();
            return Json(data);
        }
    }
}