using FreshTrackWMS.Data;
using FreshTrackWMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FreshTrackWMS.Controllers
{
    public class XuatKhoController : Controller
    {
        private const string SoDonHangPrefix = "[SoDonHang:";
        private readonly FreshTrackWmsContext _context;

        public XuatKhoController(FreshTrackWmsContext context) => _context = context;

        public async Task<IActionResult> Index()
        {
            return View(await BuildIndexViewModelAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateXuatKhoRequest request)
        {
            request.Details = request.Details
                .Where(detail => detail.MaLo > 0 && detail.SoLuong > 0)
                .GroupBy(detail => detail.MaLo)
                .Select(group => new CreateXuatKhoDetailRequest
                {
                    MaLo = group.Key,
                    SoLuong = group.Sum(detail => detail.SoLuong),
                    GhiChu = string.Join("; ", group.Select(detail => detail.GhiChu).Where(ghiChu => !string.IsNullOrWhiteSpace(ghiChu)))
                })
                .ToList();

            if (request.NguoiTao <= 0)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn người tạo.";
                return RedirectToAction(nameof(Index));
            }

            if (request.SoDonHang <= 0)
            {
                TempData["ErrorMessage"] = "Vui lòng nhập số đơn hàng lớn hơn 0.";
                return RedirectToAction(nameof(Index));
            }

            if (!request.Details.Any())
            {
                TempData["ErrorMessage"] = "Thêm không được. Vui lòng thêm ít nhất một thực phẩm tồn kho.";
                return RedirectToAction(nameof(Index));
            }

            var nguoiTaoExists = await _context.NguoiDungs.AnyAsync(nguoiDung => nguoiDung.MaNguoiDung == request.NguoiTao);
            if (!nguoiTaoExists)
            {
                TempData["ErrorMessage"] = "Không tìm thấy người tạo phù hợp.";
                return RedirectToAction(nameof(Index));
            }

            var maLoList = request.Details.Select(detail => detail.MaLo).ToList();
            var loHangs = await _context.LoHangs
                .Where(loHang => maLoList.Contains(loHang.MaLo))
                .ToDictionaryAsync(loHang => loHang.MaLo);

            foreach (var detail in request.Details)
            {
                if (!loHangs.TryGetValue(detail.MaLo, out var loHang) || loHang.SoLuong <= 0)
                {
                    TempData["ErrorMessage"] = "Thêm không được. Lô hàng đã hết tồn kho.";
                    return RedirectToAction(nameof(Index));
                }

                if (detail.SoLuong > loHang.SoLuong)
                {
                    TempData["ErrorMessage"] = "Thêm không được. Số lượng xuất lớn hơn tồn kho hiện tại.";
                    return RedirectToAction(nameof(Index));
                }
            }

            var phieuXuat = new PhieuXuat
            {
                NgayXuat = request.NgayXuat,
                LyDoXuat = request.LyDoXuat,
                NguoiTao = request.NguoiTao
            };

            for (var index = 0; index < request.Details.Count; index++)
            {
                var detail = request.Details[index];
                var ghiChu = string.IsNullOrWhiteSpace(detail.GhiChu) ? request.GhiChu : detail.GhiChu;
                if (index == 0)
                {
                    ghiChu = BuildGhiChuWithSoDonHang(request.SoDonHang, ghiChu);
                }

                phieuXuat.ChiTietPhieuXuats.Add(new ChiTietPhieuXuat
                {
                    MaLo = detail.MaLo,
                    SoLuong = detail.SoLuong,
                    GhiChu = ghiChu
                });

                loHangs[detail.MaLo].SoLuong -= detail.SoLuong;
            }

            _context.PhieuXuats.Add(phieuXuat);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Tạo phiếu xuất thành công.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<XuatKhoIndexViewModel> BuildIndexViewModelAsync()
        {
            var phieuXuatEntities = await _context.PhieuXuats
                .Include(phieuXuat => phieuXuat.NguoiTaoNavigation)
                .Include(phieuXuat => phieuXuat.ChiTietPhieuXuats)
                .OrderByDescending(phieuXuat => phieuXuat.MaPhieuXuat)
                .ToListAsync();

            var phieuXuats = phieuXuatEntities
                .Select(phieuXuat => new XuatKhoHistoryItem
                {
                    MaPhieuXuat = phieuXuat.MaPhieuXuat,
                    MaPhieu = $"PX-{phieuXuat.MaPhieuXuat:D6}",
                    NgayXuat = phieuXuat.NgayXuat,
                    LyDoXuat = phieuXuat.LyDoXuat ?? string.Empty,
                    SoDonHang = ParseSoDonHang(phieuXuat.ChiTietPhieuXuats.FirstOrDefault()?.GhiChu),
                    TongSoLuong = phieuXuat.ChiTietPhieuXuats.Sum(chiTiet => chiTiet.SoLuong),
                    NguoiTao = phieuXuat.NguoiTaoNavigation.TenTaiKhoan
                })
                .ToList();

            var tonKhoItems = await _context.LoHangs
                .Include(loHang => loHang.MaThucPhamNavigation)
                .Where(loHang => loHang.SoLuong > 0)
                .OrderBy(loHang => loHang.HanSuDung)
                .ThenBy(loHang => loHang.MaLo)
                .Select(loHang => new XuatKhoTonKhoItem
                {
                    MaLo = loHang.MaLo,
                    MaThucPham = loHang.MaThucPham,
                    TenThucPham = loHang.MaThucPhamNavigation.TenThucPham,
                    DonViTinh = loHang.MaThucPhamNavigation.DonViTinh ?? "KG",
                    SoLuongTon = loHang.SoLuong,
                    HanSuDung = loHang.HanSuDung
                })
                .ToListAsync();

            var nguoiTaoOptions = await _context.NguoiDungs
                .OrderBy(nguoiDung => nguoiDung.TenTaiKhoan)
                .Select(nguoiDung => new SelectListItem
                {
                    Value = nguoiDung.MaNguoiDung.ToString(),
                    Text = nguoiDung.TenTaiKhoan
                })
                .ToListAsync();

            return new XuatKhoIndexViewModel
            {
                PhieuXuats = phieuXuats,
                TonKhoItems = tonKhoItems,
                NguoiTaoOptions = nguoiTaoOptions,
                NewPhieuXuat = new CreateXuatKhoRequest
                {
                    NgayXuat = DateTime.Today,
                    NguoiTao = nguoiTaoOptions.FirstOrDefault() is { } firstUser ? int.Parse(firstUser.Value) : 0
                }
            };
        }

        private static string BuildGhiChuWithSoDonHang(int soDonHang, string? ghiChu)
        {
            var marker = $"{SoDonHangPrefix}{soDonHang}]";
            return string.IsNullOrWhiteSpace(ghiChu) ? marker : $"{marker} {ghiChu}";
        }

        private static int ParseSoDonHang(string? ghiChu)
        {
            if (string.IsNullOrWhiteSpace(ghiChu) || !ghiChu.StartsWith(SoDonHangPrefix))
            {
                return 0;
            }

            var endIndex = ghiChu.IndexOf(']');
            if (endIndex <= SoDonHangPrefix.Length)
            {
                return 0;
            }

            var rawValue = ghiChu.Substring(SoDonHangPrefix.Length, endIndex - SoDonHangPrefix.Length);
            return int.TryParse(rawValue, out var soDonHang) ? soDonHang : 0;
        }
    }
}
