using CourseManagement.Data;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace CourseManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ManageCoursesController : Controller
    {
        private readonly CourseManagementDbContext _context;

        public ManageCoursesController(CourseManagementDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Lấy danh sách các khóa học
            var courses = _context.KhoaHocs.ToList();
            return View(courses);
        }

        public IActionResult Details(string id)
        {
            // Tìm khóa học theo mã
            var course = _context.KhoaHocs.FirstOrDefault(k => k.MaKhoaHoc == id);

            if (course == null)
            {
                return NotFound("Khóa học không tồn tại.");
            }

            // Trả về view với thông tin chi tiết khóa học
            return View(course);
        }
    }
}