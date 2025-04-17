using CourseManagement.Data;
using CourseManagement.Models;
using CourseManagement.ViewModels.Users;
using Microsoft.AspNetCore.Mvc;

namespace CourseManagement.Areas.Users.Controllers
{
    [Area("Users")]
    public class RegisterController : Controller
    {
        private readonly CourseManagementDbContext _context;

        public RegisterController(CourseManagementDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Register()
        {
            var model = new RegisterViewModel();
            return View(model);
        }

        // POST: Handle registration form submission
        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Return the view with the current model to display validation errors
                return View(model);
            }

            // Check if email already exists
            if (_context.HocViens.Any(h => h.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Email đã được sử dụng.");
                return View(model);
            }

            // Check if phone number already exists
            if (_context.HocViens.Any(h => h.SoDienThoai == model.SoDienThoai))
            {
                ModelState.AddModelError("SoDienThoai", "Số điện thoại đã được sử dụng.");
                return View(model);
            }

            // Check if all information already exists
            if (_context.HocViens.Any(h =>
                h.HoTen == model.HoTen &&
                h.NgaySinh == model.NgaySinh &&
                h.Email == model.Email &&
                h.SoDienThoai == model.SoDienThoai))
            {
                ModelState.AddModelError(string.Empty, "Thông tin đã tồn tại trong hệ thống.");
                return View(model);
            }

            // Tạo thực thể HocVien mới
            var hocVien = new HocVien
            {
                MaHocVien = GenerateNewMaHocVien(),
                HoTen = model.HoTen,
                NgaySinh = model.NgaySinh,
                SoDienThoai = model.SoDienThoai,
                Email = model.Email,
                MatKhau = model.MatKhau,
                Role = 1 // Default role as User
            };

            // Thêm HocVien mới vào cơ sở dữ liệu
            _context.HocViens.Add(hocVien);
            _context.SaveChanges();

            // Chuyển hướng đến trang đăng nhập
            return RedirectToAction("Login", "Login");
        }

        // Method to generate a new MaHocVien
        private string GenerateNewMaHocVien()
        {
            // Get the current largest MaHocVien from the database
            var maxMaHocVien = _context.HocViens
                .OrderByDescending(h => h.MaHocVien)
                .Select(h => h.MaHocVien)
                .FirstOrDefault();

            // Extract the numeric part of MaHocVien
            int currentMax = 0;
            if (!string.IsNullOrEmpty(maxMaHocVien) && maxMaHocVien.Length > 2)
            {
                var numericPart = maxMaHocVien.Substring(2); // Skip the prefix (e.g., "HV")
                if (int.TryParse(numericPart, out var parsedNumber))
                {
                    currentMax = parsedNumber;
                }
            }

            // Increment by 1 and format as "HV" + 3-digit number
            return $"HV{(currentMax + 1):D3}";
        }
    }
}
