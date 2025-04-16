using Microsoft.AspNetCore.Mvc;
using CourseManagement.Data;
using System.Linq;
using CourseManagement.ViewModels.Users;

namespace CourseManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class MainController : Controller
    {
        private readonly CourseManagementDbContext _context;

        public MainController(CourseManagementDbContext context)
        {
            _context = context;
        }

        // Trang chính  
        public IActionResult Main()
        {
            // Lấy tổng số học viên  
            ViewBag.TotalStudents = _context.HocViens.Count();

            // Lấy số khóa học đang mở  
            ViewBag.ActiveCourses = _context.KhoaHocs.Count();

            return View();
        }

        // Quản lý học viên  
        public IActionResult ManageStudents()
        {
            // Lấy danh sách học viên  
            var students = _context.HocViens.ToList();

            // Trả về view  
            return View(students);
        }

        // Quản lý khóa học  
        public IActionResult ManageCourses()
        {

            return View();
        }

        // Chi tiết khóa học  
        public IActionResult CourseDetails(int id)
        {


            return View();
        }
    }
}
