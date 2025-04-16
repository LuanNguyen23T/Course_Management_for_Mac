using CourseManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CourseManagement.Data;

namespace CourseManagement.Areas.Admin.Controllers
{
    public class ManageStudentsController : Controller
    {
        private readonly CourseManagementDbContext _context;

        [HttpGet]
        public IActionResult AddStudent()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AddStudent(HocVien hocVien)
        {
            if (ModelState.IsValid)
            {
                _context.HocViens.Add(hocVien);
                _context.SaveChanges();
                return RedirectToAction("ManageStudents");
            }
            return View(hocVien);
        }

        [HttpGet]
        public IActionResult EditStudent(string id)
        {
            var student = _context.HocViens.FirstOrDefault(h => h.MaHocVien == id);
            if (student == null)
            {
                return NotFound();
            }
            return View(student);
        }

        [HttpPost]
        public IActionResult EditStudent(HocVien hocVien)
        {
            if (ModelState.IsValid)
            {
                _context.HocViens.Update(hocVien);
                _context.SaveChanges();
                return RedirectToAction("ManageStudents");
            }
            return View(hocVien);
        }


        [HttpGet]
        public IActionResult DeleteStudent(string id)
        {
            var student = _context.HocViens.FirstOrDefault(h => h.MaHocVien == id);
            if (student != null)
            {
                _context.HocViens.Remove(student);
                _context.SaveChanges();
            }
            return RedirectToAction("ManageStudents");
        }

    }
}
