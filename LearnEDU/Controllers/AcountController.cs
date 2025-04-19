using Microsoft.AspNetCore.Mvc;
using LearnEDU.Data;
using LearnEDU.Models;
using System.Reflection.Metadata.Ecma335;

namespace LearnEDU.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Register
        [HttpGet]
        public IActionResult Register() => View();

        // POST: Register
        [HttpPost]
        public IActionResult Register(Student student)
        {
            ModelState.Remove("Role");
            student.Role = "Student";
            student.DateRegister = DateTime.Now;
            student.CurrentBalance = 0;
            if (_context.Students.Any(s => s.Username == student.Username))
            {
                ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại.");
                return View(student);
            }

            if (_context.Students.Any(s => s.Email == student.Email))
            {
                ModelState.AddModelError("Email", "Email đã được sử dụng.");
                return View(student);
            }

            if (ModelState.IsValid)
            {
                student.Role = "Student";
                student.CurrentBalance = 0;
                student.DateRegister = DateTime.Now;

                _context.Students.Add(student);
                _context.SaveChanges();

                return RedirectToAction("Login");
            }
            if (!ModelState.IsValid)
            {
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        Console.WriteLine($"❌ Lỗi ở field '{state.Key}': {error.ErrorMessage}");
                    }
                }
            }


            return View(student);
        }



        // GET: Login
        [HttpGet]
        public IActionResult Login() => View();

        // POST: Login
        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            var user = _context.Students.FirstOrDefault(s => s.Username == username && s.Password == password);

            if (user != null)
            {
                // Ghi session
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetInt32("UserId", user.Id);
                HttpContext.Session.SetString("Role", user.Role);
                if (user.Role == "Admin")
                    return RedirectToAction("Index", "Dashboard");
                else
                    return RedirectToAction("Index", "StudentHome");
            }

            ViewBag.Error = "Sai tên đăng nhập hoặc mật khẩu.";
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
        public IActionResult AccessDenied() => View();
    }
}
