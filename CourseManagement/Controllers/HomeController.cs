using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CourseManagement.Models;
using CourseManagement.Data;
using CourseManagement.ViewModels.Users;

namespace CourseManagement.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly CourseManagementDbContext _context; 

    public HomeController(ILogger<HomeController> logger, CourseManagementDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    public IActionResult Course() 
    {
        var courses = _context.KhoaHocs
            .Select(k => new CourseViewModel
            {
                MaKhoaHoc = k.MaKhoaHoc,
                TenKhoaHoc = k.TenKhoaHoc,
                GiangVien = k.GiangVien,
                ThoiGianKhaiGiang = k.ThoiGianKhaiGiang,
                HocPhi = k.HocPhi,
                SoLuongSinhVienToiDa = k.SoLuongSinhVienToiDa
            })
            .ToList();

        return View(courses);
    }
}
