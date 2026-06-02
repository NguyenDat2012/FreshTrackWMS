// File: Controllers/DestroyProductController.cs
using FreshTrackWMS.Data;
using FreshTrackWMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FreshTrackWMS.Controllers
{
    public class DestroyProductController : Controller
    {
        private readonly FreshTrackWmsContext _context;

        public DestroyProductController(FreshTrackWmsContext context)
        {
            _context = context;
        }

        // GET: DestroyProduct
        public async Task<IActionResult> Index()
        {
            var inventoryDestructions = await _context.PhieuHuys
                .Include(p => p.NguoiTaoNavigation)
                .Include(p => p.ChiTietPhieuHuys)
                    .ThenInclude(ct => ct.MaLoNavigation)
                        .ThenInclude(l => l.MaThucPhamNavigation)
                .OrderByDescending(p => p.NgayHuy)
                .ToListAsync();

            var viewModel = inventoryDestructions.Select(p => new DestroyProductViewModel
            {
                DisplayTicketCode = p.MaPhieuHuy.ToString(),
                DestroyDate = p.NgayHuy,
                PerformedBy = p.NguoiTaoNavigation != null ? p.NguoiTaoNavigation.TenTaiKhoan : "Ẩn danh",
                TotalItems = p.ChiTietPhieuHuys.Count,
                TotalQuantity = p.ChiTietPhieuHuys.Sum(ct => ct.SoLuong),
                MainReason = p.ChiTietPhieuHuys.FirstOrDefault()?.LyDo ?? "Không có lý do"
            }).ToList();

            // Đã có sẵn trường SoLuong trong list này rồi bạn nhé
            ViewBag.AvailableBatches = await _context.LoHangs
                .Include(l => l.MaThucPhamNavigation)
                .OrderBy(l => l.HanSuDung)
                .ToListAsync();

            ViewData["Title"] = "Quản lý Hủy hàng";
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateDestroyTicketInputModel input)
        {
            try
            {
                if (input.Details == null || !input.Details.Any())
                {
                    TempData["Error"] = "Vui lòng chọn ít nhất một lô hàng để hủy.";
                    return RedirectToAction(nameof(Index));
                }

                // --- BƯỚC THẨM ĐỊNH SỐ LƯỢNG TRƯỚC KHI LƯU ---
                foreach (var item in input.Details)
                {
                    if (string.IsNullOrEmpty(item.MaLo.ToString())) continue;

                    // Truy vấn kiểm tra số lượng tồn kho của lô này trong Database hiện tại
                    var checkBatch = await _context.LoHangs.FindAsync(item.MaLo);

                    if (checkBatch == null)
                    {
                        TempData["Error"] = $"Lô hàng mã #{item.MaLo} không tồn tại trong hệ thống.";
                        return RedirectToAction(nameof(Index));
                    }

                    if (item.SoLuong <= 0)
                    {
                        TempData["Error"] = "Số lượng vật phẩm yêu cầu hủy phải lớn hơn 0.";
                        return RedirectToAction(nameof(Index));
                    }

                    // Nếu số lượng gõ trên form lớn hơn số lượng tồn thực tế trong DB
                    if (item.SoLuong > checkBatch.SoLuong)
                    {
                        TempData["Error"] = $"Không thể lập phiếu! Số lượng yêu cầu hủy ({item.SoLuong}) vượt quá số lượng tồn hiện có trong kho của Lô mã #{item.MaLo} (Tồn hiện tại: {checkBatch.SoLuong}).";
                        return RedirectToAction(nameof(Index));
                    }
                }

                // --- HẾT PHẦN KIỂM TRA -> TIẾN HÀNH LƯU VÀO DB ---
                var newTicket = new PhieuHuy
                {
                    NgayHuy = DateTime.Now
                };

                if (!string.IsNullOrEmpty(input.TenNguoiTao))
                {
                    var user = await _context.NguoiDungs
                        .FirstOrDefaultAsync(u => u.TenTaiKhoan == input.TenNguoiTao.Trim());

                    if (user != null)
                    {
                        newTicket.NguoiTao = user.MaNguoiDung;
                    }
                    else
                    {
                        TempData["Error"] = $"Không tìm thấy thông tin nhân viên '{input.TenNguoiTao}' trên cơ sở dữ liệu.";
                        return RedirectToAction(nameof(Index));
                    }
                }

                _context.PhieuHuys.Add(newTicket);
                await _context.SaveChangesAsync(); // Lưu phiếu chính an toàn

                // Lưu danh sách chi tiết thực phẩm hủy và trừ kho
                foreach (var item in input.Details)
                {
                    if (string.IsNullOrEmpty(item.MaLo.ToString())) continue;

                    var detailEntity = new ChiTietPhieuHuy
                    {
                        MaPhieuHuy = newTicket.MaPhieuHuy,
                        MaLo = item.MaLo,
                        SoLuong = item.SoLuong,
                        LyDo = item.LyDo
                    };
                    _context.ChiTietPhieuHuys.Add(detailEntity);

                    // Cập nhật trừ số lượng tồn của Lô Hàng
                    var batch = await _context.LoHangs.FindAsync(item.MaLo);
                    if (batch != null)
                    {
                        batch.SoLuong -= item.SoLuong;
                        _context.Entry(batch).State = EntityState.Modified;
                    }
                }

                await _context.SaveChangesAsync(); // Lưu toàn bộ dữ liệu chi tiết
                TempData["Success"] = "Tạo phiếu hủy hàng thành công!";
            }
            catch (Exception ex)
            {
                var innerError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                TempData["Error"] = "Lỗi hệ thống phát sinh: " + innerError;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}