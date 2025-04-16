using Microsoft.AspNetCore.Mvc;

namespace CourseManagement.Areas.Admin.Controllers
{
    public class ManageCoursesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
