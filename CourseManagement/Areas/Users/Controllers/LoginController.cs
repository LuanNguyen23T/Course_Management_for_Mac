using Microsoft.AspNetCore.Mvc;
using CourseManagement.ViewModels.Users;

using Microsoft.AspNetCore.Mvc;
using CourseManagement.ViewModels.Users;

namespace CourseManagement.Areas.Users.Controllers
{
    [Area("Users")]
    public class LoginController : Controller
    {
        [HttpGet]
        public IActionResult Login()
        {
            var model = new LoginViewModel(); // Khởi tạo model
            return View(model);
        }

        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Logic xử lý đăng nhập (nếu cần)
            return RedirectToAction("Index", "Home");
        }
    }
}