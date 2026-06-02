using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FreshTrackWMS.Data;
using FreshTrackWMS.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FreshTrackWMS.Controllers
{
    public class InventoryCheckController : Controller
    {
        private readonly FreshTrackWmsContext _context;

        public InventoryCheckController(FreshTrackWmsContext context)
        {
            _context = context;
        }

        // GET: InventoryCheck
        public async Task<IActionResult> Index(string searchString)
        {
            // Lấy danh sách Lô Hàng kèm Tên Thực Phẩm và Số tồn hệ thống ném ra UI
            var loHangs = await _context.LoHangs
                .Include(l => l.MaThucPhamNavigation)
                .Select(l => new {
                    MaLo = l.MaLo,
                    TenThucPham = l.MaThucPhamNavigation.TenThucPham,
                    Tonkho = l.SoLuong
                }).ToListAsync();

            ViewBag.Lots = loHangs;

            var query = _context.PhieuKiemKes
                .Include(p => p.NguoiTaoNavigation)
                .Include(p => p.ChiTietKiemKes)
                    .ThenInclude(c => c.MaLoNavigation)
                        .ThenInclude(l => l.MaThucPhamNavigation)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(p => p.MaPhieuKiemKe.ToString().Contains(searchString));
            }

            ViewBag.CurrentSearch = searchString;

            var tickets = await query.OrderByDescending(p => p.NgayKiemKe).ToListAsync();
            return View(tickets);
        }

        // POST: InventoryCheck/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InventoryCheckVM model)
        {
            if (ModelState.IsValid && model.Details.Any())
            {
                var userTam = await _context.NguoiDungs.FirstOrDefaultAsync();

                if (userTam == null)
                {
                    TempData["ErrorMessage"] = "Lỗi: Bảng NguoiDung trống!";
                    return RedirectToAction(nameof(Index));
                }

                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // 1. Tạo Phiếu kiểm kê
                    var phieuKiemKe = new PhieuKiemKe
                    {
                        NgayKiemKe = model.NgayKiemKe,
                        NguoiTao = userTam.MaNguoiDung
                    };
                    _context.PhieuKiemKes.Add(phieuKiemKe);
                    await _context.SaveChangesAsync();

                    // 2. Thêm Chi tiết kiểm kê
                    foreach (var item in model.Details)
                    {


                        var chiTiet = new ChiTietKiemKe
                        {
                            MaPhieuKiemKe = phieuKiemKe.MaPhieuKiemKe,
                            MaLo = item.MaLo,
                            TonHeThong = item.TonHeThong,
                            ThucTeDem = item.ThucTeDem,
                            LyDoDieuChinh = item.LyDoDieuChinh
                        };
                        _context.ChiTietKiemKes.Add(chiTiet);
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    TempData["SuccessMessage"] = "Tạo phiếu kiểm kê thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    var innerMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                    TempData["ErrorMessage"] = "Lỗi SQL: " + innerMessage;
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Dữ liệu không hợp lệ hoặc chưa thêm mặt hàng nào.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}