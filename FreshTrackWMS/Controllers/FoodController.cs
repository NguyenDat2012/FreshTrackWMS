using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FreshTrackWMS.Data;
using System.Linq;
using System.Threading.Tasks;

namespace FreshTrackWMS.Controllers
{
    public class FoodController : Controller
    {
        private readonly FreshTrackWmsContext _context;

        public FoodController(FreshTrackWmsContext context)
        {
            _context = context;
        }

        // GET: Food
        public async Task<IActionResult> Index(string searchString, string danhMuc)
        {
            var query = _context.ThucPhams.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(t => t.TenThucPham.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(danhMuc))
            {
                query = query.Where(t => t.DanhMuc == danhMuc);
            }

            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentDanhMuc = danhMuc;

            var foods = await query.OrderByDescending(t => t.MaThucPham).ToListAsync();
            return View(foods);
        }

        // POST: Food/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ThucPham thucPham)
        {
            if (ModelState.IsValid)
            {
                _context.Add(thucPham);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm thực phẩm thành công!";
                return RedirectToAction(nameof(Index));
            }
            TempData["ErrorMessage"] = "Dữ liệu không hợp lệ, vui lòng kiểm tra lại.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Food/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ThucPham thucPham)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(thucPham);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật thực phẩm thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.ThucPhams.Any(e => e.MaThucPham == thucPham.MaThucPham))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            TempData["ErrorMessage"] = "Dữ liệu chỉnh sửa không hợp lệ.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Food/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var thucPham = await _context.ThucPhams.FindAsync(id);
            if (thucPham != null)
            {
                _context.ThucPhams.Remove(thucPham);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa thực phẩm thành công!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}