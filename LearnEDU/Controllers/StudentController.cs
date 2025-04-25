using Microsoft.AspNetCore.Mvc;
using LearnEDU.Models;
using LearnEDU.Data;
using X.PagedList.Extensions;
using System.Data;

namespace LearnEDU.Controllers
{
    public class StudentController : BaseStudentController
    {
        private readonly ApplicationDbContext _context;

        public StudentController(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Add()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin")
            {
                return RedirectToAction("AccessDenied", "Account"); 
            }
            return View();
        }   
        [HttpGet]
        public IActionResult Details(int id)
        {
            var role = HttpContext.Session.GetString("Role");
            Console.WriteLine("Role: " + role);
            if (role ==null)
            {
                return RedirectToAction("AccessDenied", "Account"); 
            }
            var student = _context.Students.FirstOrDefault(s => s.Id == id);
            if (student == null)
            {
                return NotFound();
            }
            ViewBag.Role = HttpContext.Session.GetString("Role");

            return View(student);
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin")
            {
                return RedirectToAction("AccessDenied", "Account"); 
            }
            var student = _context.Students.FirstOrDefault(s => s.Id == id);
            if (student == null)
            {
                return NotFound();
            }

            if (student.Role == "Admin")
            {
                TempData["Error"] = "Không thể xóa tài khoản Admin.";
                return RedirectToAction("AllUser", "Student");
            }

            _context.Students.Remove(student);
            _context.SaveChanges();

            if(role == "Admin")
            {
                return RedirectToAction("AllUser", "Student");
            }
            else
            {
                return RedirectToAction("Index", "StudentHome");
            }
        }

        [HttpGet]
        public IActionResult AddAdmin()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin")
            {
                return RedirectToAction("AccessDenied", "Account"); 
            }
            ViewBag.IsAddAdmin = true;
            return View("Add");
        }

        [HttpPost]
        public IActionResult AddAdmin(Student student, IFormFile ImageFile)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin")
            {
                return RedirectToAction("AccessDenied", "Account"); 
            }
            ModelState.Remove("ImageFile");
            ModelState.Remove("Role");

            if (_context.Students.Any(s => s.Username == student.Username))
                ModelState.AddModelError("Username", "Tên đăng nhập đã được sử dụng.");
            if (_context.Students.Any(s => s.Email == student.Email))
                ModelState.AddModelError("Email", "Email đã được sử dụng.");

            if (!ModelState.IsValid)
            {
                ViewBag.IsAddAdmin = true;
                ViewBag.DebugErrors = ModelState
                    .Where(kvp => kvp.Value.Errors.Count > 0)
                    .Select(kvp => new
                    {
                        Field = kvp.Key,
                        Errors = kvp.Value.Errors.Select(e => e.ErrorMessage).ToList()
                    }).ToList();

                return View("Add", student);
            }

            student.DateRegister = DateTime.Now;
            student.CurrentBalance = 0;
            student.Role = "Admin";

            if (ImageFile != null && ImageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    ImageFile.CopyTo(stream);
                }

                student.Image = "/images/" + fileName;
            }

            _context.Students.Add(student);
            _context.SaveChanges();

            return RedirectToAction("AllUser");
        }

        [HttpPost]
        public IActionResult Add(Student student, IFormFile? ImageFile, string? InstructorName)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin")
            {
                return RedirectToAction("AccessDenied", "Account"); 
            }
            ModelState.Remove("DateRegister");
            ModelState.Remove("Role");

            if (_context.Students.Any(s => s.Username == student.Username))
                ModelState.AddModelError("Username", "Tên đăng nhập đã được sử dụng.");
            if (_context.Students.Any(s => s.Email == student.Email))
                ModelState.AddModelError("Email", "Email đã được sử dụng.");
            if (string.IsNullOrWhiteSpace(student.Phone) || !System.Text.RegularExpressions.Regex.IsMatch(student.Phone, @"^\d{10}$"))
                ModelState.AddModelError("Phone", "Số điện thoại phải gồm đúng 10 chữ số.");
            if (student.DateOfBirth > DateTime.Today)
                ModelState.AddModelError("DateOfBirth", "Ngày sinh không được lớn hơn ngày hiện tại.");

            if (!ModelState.IsValid)
            {
                ViewBag.DebugErrors = ModelState
                    .Where(kvp => kvp.Value.Errors.Count > 0)
                    .Select(kvp => new
                    {
                        Field = kvp.Key,
                        Errors = kvp.Value.Errors.Select(e => e.ErrorMessage).ToList()
                    }).ToList();

                return View(student);
            }

            student.DateRegister = DateTime.Now;
            student.CurrentBalance = 100;
            student.Role = "Student";

            if (ImageFile != null && ImageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    ImageFile.CopyTo(stream);
                }

                student.Image = "/images/" + fileName;
            }

            _context.Students.Add(student);
            _context.SaveChanges();

            return RedirectToAction("All");
        }

        public IActionResult All(string name, string education, string email, string phone, string gender, string username, string sortBy, string sortDir, int? page)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin")
            {
                return RedirectToAction("AccessDenied", "Account"); 
            }
            var students = _context.Students.AsQueryable();
            students = students.Where(s => s.Role == "Student");
            if (!string.IsNullOrEmpty(name))
                students = students.Where(s => (s.FirstName + " " + s.LastName).ToLower().Contains(name.ToLower().Trim()));

            if (!string.IsNullOrEmpty(education))
                students = students.Where(s => s.Education.ToLower().Contains(education.ToLower().Trim()));

            if (!string.IsNullOrEmpty(email))
                students = students.Where(s => s.Email.ToLower().Contains(email.ToLower().Trim()));

            if (!string.IsNullOrEmpty(phone))
                students = students.Where(s => s.Phone.Contains(phone.Trim()));

            if (!string.IsNullOrEmpty(gender))
                students = students.Where(s => s.Gender.ToLower().Contains(gender.ToLower().Trim()));

            if (!string.IsNullOrEmpty(username))
                students = students.Where(s => s.Username.ToLower().Contains(username.ToLower().Trim()));

            students = students.OrderBy(s => s.LastName).ThenBy(s => s.FirstName);

            int pageSize = 10;
            int pageNumber = page ?? 1;
            var pagedList = students.ToPagedList(pageNumber, pageSize);

            return View(pagedList);
        }

        public IActionResult AllUser(string username, string password, string email, string phone, string role)
        {
            var rolee = HttpContext.Session.GetString("Role");
            if (rolee != "Admin")
            {
                return RedirectToAction("AccessDenied", "Account"); 
            }
            var students = _context.Students.AsQueryable(); 

            if (!string.IsNullOrEmpty(username))
                students = students.Where(s => s.Username.Contains(username));

            if (!string.IsNullOrEmpty(password))
                students = students.Where(s => s.Password.Contains(password));

            if (!string.IsNullOrEmpty(email))
                students = students.Where(s => s.Email.Contains(email));

            if (!string.IsNullOrEmpty(phone))
                students = students.Where(s => s.Phone.Contains(phone));

            if (!string.IsNullOrEmpty(role))
                students = students.Where(s => s.Role == role);

             students = students.OrderBy(s => s.Role).ThenBy(s => s.Username);

            ViewBag.Username = username;
            ViewBag.Password = password;
            ViewBag.Email = email;
            ViewBag.Phone = phone;
            ViewBag.RoleFilter = role;

            return View("AllUser", students.ToList());
        }


        [HttpPost]
        public IActionResult Details(Student student, IFormFile? ImageFile)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role == null)
            {
                return RedirectToAction("AccessDenied", "Account"); 
            }
            ModelState.Remove("ImageFile");
            ModelState.Remove("Role");
            if (!ModelState.IsValid)
            {
                Console.WriteLine("❌ ModelState Invalid - Các lỗi gồm:");

                foreach (var state in ModelState)   
                {
                    foreach (var error in state.Value.Errors)
                    {
                        Console.WriteLine($"Lỗi ở field '{state.Key}': {error.ErrorMessage}");
                    }
                }

                return View(student);
            }

            var existingStudent = _context.Students.FirstOrDefault(s => s.Id == student.Id);
            if (existingStudent == null)
            {
                return NotFound();
            }

            bool usernameExists = _context.Students.Any(s => s.Username == student.Username && s.Id != student.Id);
            if (usernameExists)
            {
                ModelState.AddModelError("Username", "Tên đăng nhập đã được sử dụng bởi học sinh khác.");
                return View(student);
            }

            bool emailExists = _context.Students.Any(s => s.Email == student.Email && s.Id != student.Id);
            if (emailExists)
            {
                ModelState.AddModelError("Email", "Email đã được sử dụng bởi học sinh khác.");
                return View(student);
            }

            existingStudent.Username = student.Username;
            if (!string.IsNullOrWhiteSpace(student.Password)) 
            {
                existingStudent.Password = student.Password;
            }
            ViewBag.Role = HttpContext.Session.GetString("Role");
            existingStudent.FirstName = student.FirstName;
            existingStudent.LastName = student.LastName;
            existingStudent.Email = student.Email;
            existingStudent.Phone = student.Phone;
            existingStudent.Address = student.Address;
            existingStudent.DateOfBirth = student.DateOfBirth;
            existingStudent.Gender = student.Gender;
            existingStudent.Education = student.Education;
            existingStudent.CurrentBalance = student.CurrentBalance;


            if (ImageFile == null)
            {
                Console.WriteLine("❌ Không nhận được file từ form!");
            }
            else
            {
                Console.WriteLine($"✅ File nhận được: {ImageFile.FileName}, dung lượng: {ImageFile.Length}");

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    ImageFile.CopyTo(stream);
                }

                existingStudent.Image = "/images/" + fileName;

                Console.WriteLine("📸 Gán lại ảnh: " + existingStudent.Image);
            }

            _context.Students.Update(existingStudent);
            _context.SaveChanges();

            if (ViewBag.Role == "Admin")
                return RedirectToAction("Index", "Dashboard");
            else
                return RedirectToAction("Index", "StudentHome");
        }

    }
}
