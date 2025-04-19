using Microsoft.AspNetCore.Mvc;
using LearnEDU.Data;

namespace LearnEDU.Controllers
{
    public class StudentHomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StudentHomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Student")
            {
                return RedirectToAction("AccessDenied", "Account"); // hoặc về trang Home
            }

            var id = HttpContext.Session.GetInt32("UserId");
            var student = _context.Students.FirstOrDefault(s => s.Id == id);
            if (student != null)
            {
                ViewBag.CurrentBalance = student.CurrentBalance;
            }

            ViewBag.TopCourses = _context.Courses
            .OrderByDescending(c => c.CurrentSize)
            .Take(6)
            .ToList();

            ViewBag.CheapestCourses = _context.Courses
                .OrderBy(c => c.Price)
                .Take(6)
                .ToList();

            ViewBag.NewestCourses = _context.Courses
                .OrderByDescending(c => c.StartDate)
                .Take(6)
                .ToList();

            ViewBag.CoursesByCategory = _context.Courses
                .GroupBy(c => c.Category)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(c => c.CurrentSize).Take(6).ToList()
                );

            return View();
        }
    }
}
