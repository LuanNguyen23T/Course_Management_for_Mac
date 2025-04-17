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

            TempData["MaHocVien"] = user.MaHocVien; // Pass user information to ResetPassword
            return RedirectToAction("ResetPassword", "Login");
        }

        [HttpGet]
        public IActionResult ResetPassword()
        {
            var model = new LoginViewModel
            {
                MaHocVien = TempData["MaHocVien"]?.ToString()
            };

            if (string.IsNullOrEmpty(model.MaHocVien))
            {
                return RedirectToAction("ForgotPassword");
            }

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