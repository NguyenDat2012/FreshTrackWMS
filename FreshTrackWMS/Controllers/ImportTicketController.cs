using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FreshTrackWMS.Data;
using FreshTrackWMS.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FreshTrackWMS.Controllers
{
    public class ImportTicketController : Controller
    {
        private readonly FreshTrackWmsContext _context;

        public ImportTicketController(FreshTrackWmsContext context)
        {
            _context = context;
        }

        // GET: ImportTicket
        public async Task<IActionResult> Index(string searchString)
        {
            ViewBag.Suppliers = await _context.NhaCungCaps.ToListAsync();
            ViewBag.Foods = await _context.ThucPhams.ToListAsync();

            var query = _context.PhieuNhaps
                .Include(p => p.MaNhaCungCapNavigation)
                .Include(p => p.NguoiTaoNavigation)
                .Include(p => p.ChiTietPhieuNhaps)
                    .ThenInclude(c => c.MaLoNavigation) // Chọc vào bảng Lô Hàng
                        .ThenInclude(l => l.MaThucPhamNavigation) // Từ Lô Hàng chọc tiếp vào Thực Phẩm để lấy Tên
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(p => p.MaPhieuNhap.ToString().Contains(searchString));
            }

            ViewBag.CurrentSearch = searchString;

            var tickets = await query.OrderByDescending(p => p.NgayNhap).ToListAsync();
            return View(tickets);
        }


        // POST: ImportTicket/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ImportTicketVM model)
        {
            if (ModelState.IsValid && model.Details.Any())
            {
                // Bắt đầu transaction vì phải lưu nhiều bảng cùng lúc
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // Tìm và bốc duy nhất cột ID (MaNguoiDung) dưới Database lên
                    int maNguoiTao = await _context.NguoiDungs
                        .Where(u => u.TenTaiKhoan == model.NguoiTao.Trim())
                        .Select(u => u.MaNguoiDung) // Chỉ lấy đúng cột ID
                        .FirstOrDefaultAsync();    // Nếu không tìm thấy, mặc định trả về số 0

                    if (maNguoiTao <= 0)
                    {

                        TempData["ErrorMessage"] = $"Không tìm thấy nhân viên nào có tên '{model.NguoiTao}' trong hệ thống.";
                        return RedirectToAction(nameof(Index));
                    }
                   
                    // 1. Tạo Phiếu nhập
                    var phieuNhap = new PhieuNhap
                    {
                        NgayNhap = model.NgayNhap,
                        MaNhaCungCap = model.MaNhaCungCap,
                        NguoiTao = maNguoiTao,
                        GhiChu = model.GhiChu
                    };
                    _context.PhieuNhaps.Add(phieuNhap);
                    await _context.SaveChangesAsync(); // Lưu để lấy được MaPhieuNhap tự tăng

                    // 2. Tạo Lô hàng và Chi tiết phiếu nhập cho từng dòng
                    foreach (var item in model.Details)
                    {
                        // Tạo Lô hàng mới
                        var loHang = new LoHang
                        {
                            MaThucPham = item.MaThucPham,
                            // Thêm các thuộc tính khác của bảng LoHang nếu có (ví dụ: TrangThai, TonKho...)
                            NgayNhap = model.NgayNhap,
                            HanSuDung = item.HanSuDung,
                            SoLuong = item.SoLuong
                        };
                        _context.LoHangs.Add(loHang);
                        await _context.SaveChangesAsync(); // Lưu để lấy được MaLo

                        // Tạo Chi tiết phiếu nhập
                        var chiTiet = new ChiTietPhieuNhap
                        {
                            MaPhieuNhap = phieuNhap.MaPhieuNhap,
                            MaLo = loHang.MaLo,
                            SoLuong = item.SoLuong,
                            NgaySanXuat = model.NgayNhap,
                            HanSuDung = item.HanSuDung
                        };
                        _context.ChiTietPhieuNhaps.Add(chiTiet);
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    TempData["SuccessMessage"] = "Tạo phiếu nhập kho thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    // Móc thẳng lỗi sâu nhất ra để xem
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
