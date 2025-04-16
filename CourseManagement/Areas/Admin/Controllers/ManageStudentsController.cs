using CourseManagement.Models;
using Microsoft.AspNetCore.Mvc;
using CourseManagement.Data;

namespace CourseManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ManageStudentsController : Controller
    {
        private readonly CourseManagementDbContext _context;

        public ManageStudentsController(CourseManagementDbContext context)
        {
            _context = context;
        }

        // Thêm học viên (GET)
        [HttpGet]
        public IActionResult AddStudent()
        {
            return View();
        }

        // Thêm học viên (POST)
        [HttpPost]
        public IActionResult AddStudent(HocVien hocVien)
        {
            if (ModelState.IsValid)
            {
                _context.HocViens.Add(hocVien);
                _context.SaveChanges();
                return RedirectToAction("Index", "Main", new { area = "Admin" });
            }
            return View(hocVien);
        }

        // Sửa thông tin học viên (GET)
        [HttpGet]
        public IActionResult EditStudent(string id)
        {
            var student = _context.HocViens.FirstOrDefault(h => h.MaHocVien == id);
            if (student == null)
            {
                return NotFound("Học viên không tồn tại.");
            }
            return View(student);
        }

        // Sửa thông tin học viên (POST)
        [HttpPost]
        public IActionResult EditStudent(HocVien hocVien)
        {
            if (ModelState.IsValid)
            {
                _context.HocViens.Update(hocVien);
                _context.SaveChanges();
                return RedirectToAction("Index", "Main", new { area = "Admin" });
            }
            return View(hocVien);
        }

        [HttpPost]
        public IActionResult DeleteStudent(string id)
        {
            var student = _context.HocViens.FirstOrDefault(h => h.MaHocVien == id);
            if (student != null)
            {
                _context.HocViens.Remove(student);
                _context.SaveChanges();
            }
            return RedirectToAction("ManageStudents", "ManageStudents", new { area = "Admin" });
        }
    }
}