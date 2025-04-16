using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace CourseManagement.Areas.Users.Controllers
{
    [Area("Users")]
    public class AuthenticationController : Controller
    {
        [HttpGet]
        public IActionResult IsLoggedIn()
        {
            // Kiểm tra xem người dùng đã đăng nhập hay chưa
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                // Lấy thông tin từ claims
                var username = User.Claims.FirstOrDefault(c => c.Type == "Username")?.Value;
                return Json(new { IsLoggedIn = true, Username = username });
            }

            return Json(new { IsLoggedIn = false });
        }
    }
}
