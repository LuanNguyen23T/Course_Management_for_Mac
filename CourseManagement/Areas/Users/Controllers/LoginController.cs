using Microsoft.AspNetCore.Mvc;
using CourseManagement.ViewModels.Users;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using CourseManagement.Data;

namespace CourseManagement.Areas.Users.Controllers
{
    [Area("Users")]
    public class LoginController : Controller
    {
        private readonly CourseManagementDbContext _context;

        public LoginController(CourseManagementDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            var model = new LoginViewModel(); // Khởi tạo model
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = _context.HocViens.FirstOrDefault(h => h.MaHocVien == model.MaHocVien);
            if (user == null || user.MatKhau != model.MatKhau)
            {
                ModelState.AddModelError(string.Empty, "Mã học viên hoặc mật khẩu không đúng.");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.MaHocVien),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            var model = new LoginViewModel(); // Initialize the model
            return View(model);
        }

        [HttpPost]
        public IActionResult ForgotPassword(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = _context.HocViens.FirstOrDefault(h => h.MaHocVien == model.MaHocVien && h.Email == model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Mã học viên hoặc email không đúng.");
                return View(model);
            }

            // Simulate sending an email with a reset link
            TempData["MaHocVien"] = user.MaHocVien; // Store MaHocVien in TempData
            TempData["Message"] = "Một liên kết đặt lại mật khẩu đã được gửi đến email của bạn.";
            
            // Redirect to ResetPassword page
            return RedirectToAction("ResetPassword");
        }

        [HttpGet]
        public IActionResult NewResetPasswordPage()
        {
            if (!TempData.ContainsKey("MaHocVien"))
            {
                return RedirectToAction("ForgotPassword");
            }

            var maHocVien = TempData["MaHocVien"]?.ToString();
            TempData["MaHocVien"] = maHocVien; // Reassign to ensure persistence

            var model = new LoginViewModel
            {
                MaHocVien = maHocVien
            };

            return View(model); // Render a new view for resetting the password
        }

        [HttpGet]
        public IActionResult ResetPassword()
        {
            if (!TempData.ContainsKey("MaHocVien"))
            {
                return RedirectToAction("ForgotPassword");
            }

            var maHocVien = TempData["MaHocVien"]?.ToString();
            TempData["MaHocVien"] = maHocVien; // Reassign to ensure persistence

            var model = new LoginViewModel
            {
                MaHocVien = maHocVien
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult ResetPassword(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = _context.HocViens.FirstOrDefault(h => h.MaHocVien == model.MaHocVien);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Người dùng không tồn tại.");
                return View(model);
            }

            user.MatKhau = model.NewPassword; 
            _context.SaveChanges();

            TempData["Message"] = "Mật khẩu đã được thay đổi thành công.";
            return RedirectToAction("Login");
        }
    }
}