using Microsoft.AspNetCore.Mvc;
using LearnEDU.Data;
using System;
using System.Collections.Generic;
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
            return RedirectToAction("AccessDenied", "Account");
        }

        fromDate ??= DateTime.Today.AddDays(-7);
        toDate ??= DateTime.Today;
        toDate = toDate.Value.AddDays(1).AddTicks(-1); 

        ViewBag.FromDate = fromDate.Value.ToString("yyyy-MM-dd");
        ViewBag.ToDate = toDate.Value.ToString("yyyy-MM-dd");

        ViewBag.TotalStudents = _context.Students.Count(s => s.Role == "Student");
        ViewBag.TotalCourses = _context.Courses.Count();

        var allDates = Enumerable.Range(0, (toDate.Value - fromDate.Value).Days + 1)
            .Select(offset => fromDate.Value.AddDays(offset).Date)
            .ToList();

        var revenueByCourse = _context.Enrollments
            .Where(e => e.EnrollDate >= fromDate && e.EnrollDate <= toDate)
            .GroupBy(e => new { e.Course.Name, Date = e.EnrollDate.Date })
            .Select(g => new
            {
                g.Key.Name,
                g.Key.Date,
                DailyRevenue = g.Sum(e => e.Course.Price)
            })
            .ToList();

        var groupedData = revenueByCourse
            .GroupBy(r => r.Name)
            .Select(g =>
            {
                var dailyRevenueByDate = allDates
                    .Select(date => new
                    {
                        Date = date.ToString("yyyy-MM-dd"),
                        Revenue = g.FirstOrDefault(x => x.Date == date)?.DailyRevenue ?? 0
                    })
                    .ToList();

                return new
                {
                    CourseName = g.Key,
                    Dates = dailyRevenueByDate.Select(x => x.Date).ToList(),
                    DailyRevenue = dailyRevenueByDate.Select(x => x.Revenue).ToList()
                };
            })
            .ToList();

        ViewBag.RevenueByCourse = groupedData;

        ViewBag.TotalRevenue = revenueByCourse.Sum(x => x.DailyRevenue);

        var totalRevenueByCourse = revenueByCourse
            .GroupBy(r => r.Name)
            .Select(g => new
            {
                CourseName = g.Key,
                TotalRevenue = g.Sum(x => x.DailyRevenue)
            })
            .ToList();

        ViewBag.TotalRevenueByCourse = totalRevenueByCourse;

        return View();
    }
}