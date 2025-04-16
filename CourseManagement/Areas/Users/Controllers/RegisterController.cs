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
                // Nếu xác thực thất bại, trả về view với model hiện tại
                return View(model);
            }

            // Kiểm tra email đã tồn tại
            if (_context.HocViens.Any(h => h.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Email đã được sử dụng.");
                return View(model);
            }

            // Kiểm tra số điện thoại đã tồn tại
            if (_context.HocViens.Any(h => h.SoDienThoai == model.SoDienThoai))
            {
                ModelState.AddModelError("SoDienThoai", "Số điện thoại đã được sử dụng.");
                return View(model);
            }

            // Kiểm tra toàn bộ thông tin (nếu cần)
            if (_context.HocViens.Any(h =>
                h.HoTen == model.HoTen &&
                h.NgaySinh == model.NgaySinh &&
                h.Email == model.Email &&
                h.SoDienThoai == model.SoDienThoai))
            {
                ModelState.AddModelError(string.Empty, "Thông tin đã tồn tại trong hệ thống.");
                return View(model);
            }

            // Tạo mã học viên mới
            string newMaHocVien = GenerateNewMaHocVien();

            // Tạo thực thể HocVien mới
            var hocVien = new HocVien
            {
                MaHocVien = newMaHocVien,
                HoTen = model.HoTen,
                NgaySinh = model.NgaySinh,
                SoDienThoai = model.SoDienThoai,
                Email = model.Email,
                MatKhau = model.MatKhau // Đảm bảo mã hóa mật khẩu trong ứng dụng thực tế
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

            // Convert the current largest MaHocVien to an integer
            int currentMax = string.IsNullOrEmpty(maxMaHocVien) ? 0 : int.Parse(maxMaHocVien);

            // Increment by 1 and format as a 9-digit string
            return (currentMax + 1).ToString("D9");
        }
    }
}
