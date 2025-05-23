﻿using Microsoft.AspNetCore.Mvc;
using LearnEDU.Models;
using LearnEDU.Data;
using X.PagedList.Extensions;
using Microsoft.EntityFrameworkCore;

namespace LearnEDU.Controllers
{
    public class CourseController : BaseStudentController
    {
        private readonly ApplicationDbContext _context;

        public CourseController(ApplicationDbContext context) : base(context)
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

            ViewBag.Instructors = _context.Courses
                .Select(c => c.InstructorName)
                .Where(name => !string.IsNullOrEmpty(name)) 
                .Distinct() 
                .ToList();
            ViewBag.Categories = GetCategoryList();
            ViewBag.Levels = GetLevelList(); 
            return View();
        }
        [HttpGet]
        public IActionResult All(
            string name,
            DateTime? startDate,
            string instructor,
            string level,
            string category,
            string priceRange,
            int? page = 1)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role == null)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            ViewBag.Role = role;
            var courses = _context.Courses.AsQueryable();

            if (!string.IsNullOrEmpty(name))
                courses = courses.Where(c => c.Name.ToLower().Contains(name.ToLower()));

            if (startDate.HasValue)
                courses = courses.Where(c => c.StartDate >= startDate.Value);

            if (!string.IsNullOrEmpty(instructor))
                courses = courses.Where(c => c.InstructorName.ToLower().Contains(instructor.ToLower()));

            if (!string.IsNullOrEmpty(level))
                courses = courses.Where(c => c.Level == level);

            if (!string.IsNullOrEmpty(category))
                courses = courses.Where(c => c.Category == category);

            if (!string.IsNullOrEmpty(priceRange))
            {
                switch (priceRange)
                {
                    case "lt500":
                        courses = courses.Where(c => c.Price < 500);
                        break;
                    case "500to1000":
                        courses = courses.Where(c => c.Price >= 500 && c.Price <= 1000); 
                        break;
                    case "gt1000":
                        courses = courses.Where(c => c.Price > 1000);
                        break;
                }
            }

            courses = courses.OrderBy(c => c.StartDate);

            int pageSize = 6;
            int pageNumber = page ?? 1;

            ViewBag.Levels = GetLevelList();
            ViewBag.Categories = GetCategoryList();

            ViewBag.SearchName = name;
            ViewBag.SearchDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.SelectedInstructor = instructor;
            ViewBag.SelectedLevel = level;
            ViewBag.SelectedCategory = category;
            ViewBag.SelectedPriceRange = priceRange;

            return View(courses.ToPagedList(pageNumber, pageSize));
        }


        [HttpGet]
        public IActionResult Details(int id)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role == null)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var course = _context.Courses.FirstOrDefault(c => c.Id == id);
            if (course == null) return NotFound();

            var studentId = HttpContext.Session.GetInt32("UserId");
            bool isEnrolled = false;

            if (studentId != null)
            {
                isEnrolled = _context.Enrollments.Any(e => e.CourseId == id && e.StudentId == studentId);
            }
            var student = _context.Students.FirstOrDefault(s => s.Id == studentId);
            if (student != null)
            {
                ViewBag.CurrentBalance = student.CurrentBalance;
            }

            ViewBag.Role = HttpContext.Session.GetString("Role");
            ViewBag.IsEnrolled = isEnrolled;

            return View(course);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin")
            {
                return RedirectToAction("AccessDenied", "Account"); 
            }

            var course = _context.Courses.FirstOrDefault(c => c.Id == id);
            if (course == null) return NotFound();

            ViewBag.Instructors = _context.Courses
                .Select(c => c.InstructorName)
                .Where(name => !string.IsNullOrEmpty(name)) 
                .Distinct() 
                .ToList();
            ViewBag.Categories = GetCategoryList();
            ViewBag.Levels = GetLevelList();

            return View(course);
        }
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin")
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var course = _context.Courses.FirstOrDefault(c => c.Id == id);
            if (course == null) return NotFound();

            if (course.CurrentSize > 0)
            {
                TempData["Error"] = "❌ Không thể xóa khóa học vì đã có học sinh đăng ký.";
                return RedirectToAction("All");
            }

            _context.Courses.Remove(course);
            _context.SaveChanges();

            TempData["Success"] = "✅ Đã xóa khóa học thành công.";
            return RedirectToAction("All");
        }

        [HttpGet]
        public IActionResult EnrolledStudents(int id)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role == null )
            {
                return RedirectToAction("AccessDenied", "Account"); 
            }
            var course = _context.Courses.FirstOrDefault(c => c.Id == id);
            if (course == null) return NotFound();

            var enrollments = _context.Enrollments
                .Where(e => e.CourseId == id)
                .Include(e => e.Student)
                .ToList();

            ViewBag.CourseName = course.Name;
            ViewBag.Role = role;
            ViewBag.CourseId = id;
            return View(enrollments);
        }


        [HttpPost]
        public IActionResult Enroll(int id)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Student")
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var studentId = HttpContext.Session.GetInt32("UserId");
            if (studentId == null) return RedirectToAction("Login", "Account");

            var student = _context.Students.FirstOrDefault(s => s.Id == studentId);
            var course = _context.Courses.FirstOrDefault(c => c.Id == id);

            if (student == null || course == null) return NotFound();

            if (DateTime.Now >= course.StartDate)
            {
                TempData["EnrollError"] = "Khóa học đã bắt đầu. Không thể ghi danh.";
                return RedirectToAction("Details", new { id });
            }

            if (course.CurrentSize >= course.Capacity)
            {
                TempData["EnrollError"] = "Khóa học đã đầy.";
                return RedirectToAction("Details", new { id });
            }

            if (_context.Enrollments.Any(e => e.StudentId == student.Id && e.CourseId == id))
            {
                TempData["EnrollError"] = "Bạn đã ghi danh khóa học này.";
                return RedirectToAction("Details", new { id });
            }

            if (student.CurrentBalance < course.Price)
            {
                TempData["EnrollError"] = "Bạn không đủ tiền để ghi danh.";
                return RedirectToAction("Details", new { id });
            }

            try
            {
                student.CurrentBalance -= course.Price;
                course.CurrentSize++;
                _context.Enrollments.Add(new Enrollment { StudentId = student.Id, CourseId = id, EnrollDate = DateTime.Now });
                _context.SaveChanges();

                TempData["EnrollSuccess"] = "Đã ghi danh thành công.";
            }
            catch (Exception ex)
            {
                TempData["EnrollError"] = "Đã xảy ra lỗi khi ghi danh. Vui lòng thử lại.";
                Console.WriteLine($"❌ Error during enrollment: {ex.Message}");
            }

            return RedirectToAction("Details", new { id });
        }

        [HttpPost]
        public IActionResult Unenroll(int courseId, int studentId)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role == null)
            {
                return RedirectToAction("AccessDenied", "Account"); 
            }

            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (role != "Admin" && currentUserId != studentId)
            {
                TempData["EnrollError"] = "Bạn không có quyền hủy đăng ký học viên này.";
                return RedirectToAction("Details", new { id = courseId });
            }

            var enrollment = _context.Enrollments.FirstOrDefault(e => e.CourseId == courseId && e.StudentId == studentId);
            var student = _context.Students.FirstOrDefault(s => s.Id == studentId);
            var course = _context.Courses.FirstOrDefault(c => c.Id == courseId);

            if (enrollment == null || student == null || course == null)
            {
                TempData["EnrollError"] = "Không thể hủy ghi danh.";
                return RedirectToAction("EnrolledStudents", new { id = courseId });
            }

            if (DateTime.Now >= course.StartDate)
            {
                TempData["EnrollError"] = "Khóa học đã bắt đầu. Không thể hủy ghi danh.";
                return RedirectToAction("EnrolledStudents", new { id = courseId });
            }

            _context.Enrollments.Remove(enrollment);
            student.CurrentBalance += course.Price;
            course.CurrentSize--;
            _context.SaveChanges();

            TempData["EnrollSuccess"] = "Đã hủy ghi danh thành công.";
            return RedirectToAction("EnrolledStudents", new { id = courseId });
        }


        [HttpPost]
        public IActionResult Edit(Course course, IFormFile ImageFile)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin")
            {
                return RedirectToAction("AccessDenied", "Account"); 
            }
            ModelState.Remove("ImageFile");

            if (course.Capacity < 0)
                ModelState.AddModelError("Capacity", "Sĩ số không thể âm");

            if (course.Price < 0)
                ModelState.AddModelError("Price", "Giá không thể âm");

            if (course.StartDate > course.EndDate)
            {
                ModelState.AddModelError("StartDate", "Ngày bắt đầu phải trước ngày kết thúc");
                ModelState.AddModelError("EndDate", "Ngày kết thúc phải sau ngày bắt đầu");
            }

            if (!ModelState.IsValid)
            {
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        Console.WriteLine($"❌ Field '{state.Key}': {error.ErrorMessage}");
                    }
                }
                ViewBag.Instructors = _context.Courses
                    .Select(c => c.InstructorName)
                    .Where(name => !string.IsNullOrEmpty(name)) 
                    .Distinct() 
                    .ToList();
                ViewBag.Categories = GetCategoryList();
                ViewBag.Levels = GetLevelList();
                return View(course);
            }

            var existing = _context.Courses.FirstOrDefault(c => c.Id == course.Id);
            if (existing == null) return NotFound();

            existing.Name = course.Name;
            existing.Description = course.Description;
            existing.Content = course.Content;
            existing.Category = course.Category;
            existing.InstructorName = course.InstructorName;
            existing.Level = course.Level;
            existing.StartDate = course.StartDate;
            existing.EndDate = course.EndDate;
            existing.Capacity = course.Capacity;
            existing.CurrentSize = course.CurrentSize;
            existing.Price = course.Price;
            existing.DateCreated = course.DateCreated;
            existing.Duration = (course.EndDate - course.StartDate).Days;

            if (ImageFile != null && ImageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    ImageFile.CopyTo(stream);
                }

                existing.ImageUrl = "/images/" + fileName;
            }

            _context.Courses.Update(existing);
            _context.SaveChanges();

            if (role == "Admin")
                return RedirectToAction("Index", "Dashboard");
            else
                return RedirectToAction("Index", "StudentHome");
        }



        [HttpPost]
        public IActionResult Add(Course course, IFormFile ImageFile)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin")
            {
                return RedirectToAction("AccessDenied", "Account"); 
            }
            ModelState.Remove("ImageFile");
            ModelState.Remove("DateCreated");

            bool isNameExists = false;
            if (!string.IsNullOrWhiteSpace(course.Name))
            {
                isNameExists = _context.Courses.Any(c => c.Name.ToLower() == course.Name.ToLower());
                if (isNameExists)
                {
                    ModelState.AddModelError("Name", "Tên khóa học đã tồn tại.");
                }
            }
            if (isNameExists)
            {
                ModelState.AddModelError("Name", "Tên khóa học đã tồn tại.");
            }
            if (course.Capacity < 0)
            {
                ModelState.AddModelError("Capacity", "Số lượng không thể nhỏ hơn 0.");
            }

            if (course.Price < 0)
            {
                ModelState.AddModelError("Price", "Giá không được nhỏ hơn 0.");
            }

            if (course.StartDate > course.EndDate)
            {
                ModelState.AddModelError("StartDate", "Ngày bắt đầu không được trễ hơn ngày kết thúc.");
                ModelState.AddModelError("EndDate", "Ngày kết thúc phải sau ngày bắt đầu.");
            }

            if (ModelState.IsValid)
            {
                course.DateCreated = DateTime.Now;
                course.CurrentSize = 0; 
                course.Duration = (course.EndDate - course.StartDate).Days;

                if (ImageFile != null && ImageFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        ImageFile.CopyTo(stream);
                    }

                    course.ImageUrl = "/images/" + fileName;
                }

                _context.Courses.Add(course);
                _context.SaveChanges();

                return RedirectToAction("All");
            }

            ViewBag.Categories = GetCategoryList();
            ViewBag.Levels = GetLevelList(); 

            return View(course);
        }

        public IActionResult MyCourse(
            string name,
            DateTime? startDate,
            string instructor,
            string level,
            string category,
            string priceRange,
            bool onlyUpcoming = false,
            int? page = 1)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Student")
            {
                return RedirectToAction("AccessDenied", "Account"); 
            }
            int? studentId = HttpContext.Session.GetInt32("UserId");
            if (studentId == null) return RedirectToAction("Login", "Account");

            var enrollments = _context.Enrollments
                .Where(e => e.StudentId == studentId)
                .Include(e => e.Course)
                .OrderByDescending(e => e.EnrollDate)
                .ToList();

            var enrollDates = enrollments.ToDictionary(
                e => e.Course.Id,
                e => e.EnrollDate.ToString("HH:mm:ss dd/MM/yyyy")
            );
            ViewBag.CourseEnrollDates = enrollDates;

            var courseQuery = enrollments.Select(e => e.Course).AsQueryable();

            if (!string.IsNullOrEmpty(name))
                courseQuery = courseQuery.Where(c => c.Name.ToLower().Contains(name.ToLower()));

            if (startDate.HasValue)
                courseQuery = courseQuery.Where(c => c.StartDate >= startDate.Value);

            if (!string.IsNullOrEmpty(instructor))
                courseQuery = courseQuery.Where(c => c.InstructorName.ToLower().Contains(instructor.ToLower())); 

            if (!string.IsNullOrEmpty(level))
                courseQuery = courseQuery.Where(c => c.Level == level);

            if (!string.IsNullOrEmpty(category))
                courseQuery = courseQuery.Where(c => c.Category == category);

            if (!string.IsNullOrEmpty(priceRange))
            {
                switch (priceRange)
                {
                    case "lt500":
                        courseQuery = courseQuery.Where(c => c.Price < 500);
                        break;
                    case "500to1000":
                        courseQuery = courseQuery.Where(c => c.Price >= 500 && c.Price <= 1000);
                        break;
                    case "gt1000":
                        courseQuery = courseQuery.Where(c => c.Price > 1000);
                        break;
                }
            }

            if (onlyUpcoming)
                courseQuery = courseQuery.Where(c => c.StartDate > DateTime.Now);

            ViewBag.MyCourseMode = true;
            ViewBag.Levels = GetLevelList();
            ViewBag.Categories = GetCategoryList();

            ViewBag.SearchName = name;
            ViewBag.SearchDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.SelectedInstructor = instructor;
            ViewBag.SelectedLevel = level;
            ViewBag.SelectedCategory = category;
            ViewBag.SelectedPriceRange = priceRange;
            ViewBag.OnlyUpcoming = onlyUpcoming;

            int pageSize = 9;
            int pageNumber = page ?? 1;

            return View("All", courseQuery.ToPagedList(pageNumber, pageSize));
        }

        private List<string> GetLevelList()
        {
            return new List<string> { 
                "Cơ bản", 
                "Trung cấp", 
                "Cao cấp" 
            };
        }

        private List<string> GetCategoryList()
        {
            return new List<string>
            {
                "IT",
                "Tiếng Nhật",
            };
        }
    }
}
