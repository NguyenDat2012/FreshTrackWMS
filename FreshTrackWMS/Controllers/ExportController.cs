using FreshTrackWMS.Data;
using FreshTrackWMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FreshTrackWMS.Controllers
{
    public class ExportController : Controller
    {
        private readonly FreshTrackWmsContext _context;
        private const int PageSize = 10;

        public ExportController(FreshTrackWmsContext context)
        {
            _context = context;
        }

        // 1. HIỂN THỊ DANH SÁCH LỊCH SỬ XUẤT KHO
        public async Task<IActionResult> Index(string searchTerm, int page = 1)
        {
            var query = _context.PhieuXuats
                .Include(p => p.NguoiTaoNavigation)
                .Include(p => p.ChiTietPhieuXuats)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.Trim();
                query = query.Where(p => p.MaPhieuXuat.ToString() == searchTerm || p.LyDoXuat.Contains(searchTerm));
            }

            var rawTickets = await query.OrderByDescending(p => p.NgayXuat).ToListAsync();

            var mappedTickets = rawTickets.Select(p => new ExportTicketItemViewModel
            {
                MaPhieuXuat = p.MaPhieuXuat,
                NgayXuat = p.NgayXuat,
                LyDoXuat = p.LyDoXuat ?? "Không có lý do",
                SoMatHang = p.ChiTietPhieuXuats.Count,
                TongSoLuong = p.ChiTietPhieuXuats.Sum(c => c.SoLuong),
                TenNguoiTao = p.NguoiTaoNavigation?.TenTaiKhoan ?? "Hệ thống"
            }).ToList();

            int totalRecords = mappedTickets.Count;
            int totalPages = (int)Math.Ceiling((double)totalRecords / PageSize);
            if (page < 1) page = 1;

            var pagedTickets = mappedTickets.Skip((page - 1) * PageSize).Take(PageSize).ToList();

            // ĐỒNG BỘ GIỐNG PHIẾU HỦY: Lấy thẳng danh sách lô hàng kèm thông tin thực phẩm
            ViewBag.AvailableBatches = await _context.LoHangs
                .Include(l => l.MaThucPhamNavigation)
                .Where(l => l.SoLuong > 0) // Chỉ lấy lô còn hàng để xuất
                .OrderBy(l => l.HanSuDung) // Gợi ý FEFO (Hạn gần nhất lên đầu)
                .ToListAsync();

            var viewModel = new ExportPageViewModel
            {
                Tickets = pagedTickets,
                SearchTerm = searchTerm,
                CurrentPage = page,
                TotalPages = totalPages
            };

            return View(viewModel);
        }

        // 2. XỬ LÝ LƯU PHIẾU XUẤT KHO VÀ TRỪ SỐ LƯỢNG LÔ HÀNG (Logic chuẩn gọn như Phiếu Hủy)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateExportInputModel input)
        {
            try
            {
                if (input.Details == null || !input.Details.Any())
                {
                    TempData["Error"] = "Vui lòng chọn ít nhất một lô hàng thực phẩm để xuất.";
                    return RedirectToAction(nameof(Index));
                }

                // Kiểm tra thẩm định số lượng kho trước khi thực hiện giao dịch lưu
                foreach (var item in input.Details)
                {
                    if (item.MaLo <= 0) continue;

                    var checkBatch = await _context.LoHangs.FindAsync(item.MaLo);
                    if (checkBatch == null)
                    {
                        TempData["Error"] = $"Lô hàng mã #{item.MaLo} không tồn tại trên hệ thống.";
                        return RedirectToAction(nameof(Index));
                    }

                    if (item.SoLuongXuat <= 0)
                    {
                        TempData["Error"] = "Số lượng vật phẩm yêu cầu xuất phải lớn hơn 0.";
                        return RedirectToAction(nameof(Index));
                    }

                    if (item.SoLuongXuat > checkBatch.SoLuong)
                    {
                        TempData["Error"] = $"Không thể lập phiếu! Số lượng xuất ({item.SoLuongXuat}) vượt quá lượng tồn kho khả dụng của mã lô #{item.MaLo} (Hiện tồn: {checkBatch.SoLuong}).";
                        return RedirectToAction(nameof(Index));
                    }
                }

                int maNguoiDung = await _context.NguoiDungs
                    .Where(u => u.TenTaiKhoan == input.TenNguoiTao.Trim())
                    .Select(u => u.MaNguoiDung)
                    .FirstOrDefaultAsync();

                if (maNguoiDung == 0)
                {
                    TempData["Error"] = $"Không tìm thấy thông tin tài khoản nhân viên lập phiếu '{input.TenNguoiTao}'.";
                    return RedirectToAction(nameof(Index));
                }

                // Khởi tạo thực thể Phiếu Xuất chính
                var phieuXuat = new PhieuXuat
                {
                    NgayXuat = input.NgayXuat,
                    LyDoXuat = input.LyDoXuat,
                    NguoiTao = maNguoiDung
                };
                _context.PhieuXuats.Add(phieuXuat);
                await _context.SaveChangesAsync();

                // Lưu danh sách chi tiết thực phẩm xuất và trừ kho trực tiếp
                foreach (var item in input.Details)
                {
                    if (item.MaLo <= 0) continue;

                    var chiTiet = new ChiTietPhieuXuat
                    {
                        MaPhieuXuat = phieuXuat.MaPhieuXuat,
                        MaLo = item.MaLo,
                        SoLuong = item.SoLuongXuat,
                        GhiChu = input.GhiChuTongQuan
                    };
                    _context.ChiTietPhieuXuats.Add(chiTiet);

                    // Cập nhật trừ số lượng tồn của Lô Hàng gốc
                    var batch = await _context.LoHangs.FindAsync(item.MaLo);
                    if (batch != null)
                    {
                        batch.SoLuong -= item.SoLuongXuat;
                        _context.Entry(batch).State = EntityState.Modified;
                    }
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Tạo phiếu xuất kho thành công! Hệ thống đã tự động khấu trừ số lượng các lô hàng.";
            }
            catch (Exception ex)
            {
                var innerError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                TempData["Error"] = "Lỗi xuất kho phát sinh: " + innerError;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}