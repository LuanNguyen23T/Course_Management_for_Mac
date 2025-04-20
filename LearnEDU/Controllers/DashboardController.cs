using Microsoft.AspNetCore.Mvc;
using LearnEDU.Data;
using System;
using System.Linq;

public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;

    public DashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index(DateTime? fromDate, DateTime? toDate)
    {
        var role = HttpContext.Session.GetString("Role");
        if (role != "Admin")
        {
            return RedirectToAction("AccessDenied", "Account"); // hoặc về trang Home
        }
        fromDate ??= DateTime.Today.AddDays(-7);
        toDate ??= DateTime.Today;
        toDate = toDate.Value.AddDays(1).AddTicks(-1); // 2025-04-19 23:59:59.9999999

        ViewBag.FromDate = fromDate.Value.ToString("yyyy-MM-dd");
        ViewBag.ToDate = toDate.Value.ToString("yyyy-MM-dd");

        // Tổng học sinh, tổng khóa học
        ViewBag.TotalStudents = _context.Students.Count(s => s.Role == "Student");
        ViewBag.TotalCourses = _context.Courses.Count();

        // Doanh thu từng khóa học trong khoảng thời gian
        var revenueByCourse = _context.Enrollments
            .Where(e => e.EnrollDate >= fromDate && e.EnrollDate <= toDate)
            .GroupBy(e => e.Course.Name)
            .Select(g => new
            {
                CourseName = g.Key,
                Revenue = g.Sum(e => e.Course.Price)
            })
            .OrderByDescending(x => x.Revenue)
            .ToList();

        ViewBag.RevenueByCourse = revenueByCourse;
        var totalRevenue = _context.Enrollments
            .Where(e => e.EnrollDate >= fromDate && e.EnrollDate <= toDate)
            .Sum(e => e.Course.Price);
        ViewBag.TotalRevenue = totalRevenue;

        return View();
    }
}
