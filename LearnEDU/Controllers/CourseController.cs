using Microsoft.AspNetCore.Mvc;
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
                return RedirectToAction("AccessDenied", "Account"); // hoặc về trang Home
            }

            ViewBag.Instructors = GetInstructorList();
            ViewBag.Categories = GetCategoryList();
            ViewBag.Levels = GetLevelList(); 
            return View();
        }
        [HttpGet]
        [HttpGet]
        public IActionResult All(
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
            if (role == null)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            ViewBag.Role = role;
            var courses = _context.Courses.AsQueryable();

            // 🔍 Lọc theo các trường nhập
            if (!string.IsNullOrEmpty(name))
                courses = courses.Where(c => c.Name.ToLower().Contains(name.ToLower()));

            if (startDate.HasValue)
                courses = courses.Where(c => c.StartDate >= startDate.Value);

            if (!string.IsNullOrEmpty(instructor))
                courses = courses.Where(c => c.InstructorName == instructor);

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

            if (onlyUpcoming)
                courses = courses.Where(c => c.StartDate > DateTime.Now);

            // ✅ Mặc định sắp xếp theo StartDate
            courses = courses.OrderBy(c => c.StartDate);

            // 📄 Phân trang
            int pageSize = 9;
            int pageNumber = page ?? 1;

            // 🧾 Gán lại dropdown list
            ViewBag.Instructors = GetInstructorList();
            ViewBag.Levels = GetLevelList();
            ViewBag.Categories = GetCategoryList();

            // 📌 Gửi lại filter để giữ UI
            ViewBag.SearchName = name;
            ViewBag.SearchDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.SelectedInstructor = instructor;
            ViewBag.SelectedLevel = level;
            ViewBag.SelectedCategory = category;
            ViewBag.SelectedPriceRange = priceRange;
            ViewBag.OnlyUpcoming = onlyUpcoming.ToString().ToLower();

            return View(courses.ToPagedList(pageNumber, pageSize));
        }


        [HttpGet]
        public IActionResult Details(int id)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role == null)
            {
                return RedirectToAction("AccessDenied", "Account"); // hoặc về trang Home
            }

            var course = _context.Courses.FirstOrDefault(c => c.Id == id);
            if (course == null) return NotFound();

            var studentId = HttpContext.Session.GetInt32("UserId");
            bool isEnrolled = false;

            if (studentId != null)
            {
                  //Neu tim thay 1 bang ghi trong Enrollment thoa man
                isEnrolled = _context.Enrollments.Any(e => e.CourseId == id && e.StudentId == studentId);
            }
            var student = _context.Students.FirstOrDefault(s => s.Id == studentId);
            if (student != null)
            {
                ViewBag.CurrentBalance = student.CurrentBalance;
            }

            //Truyen vao de sang View su dung
            ViewBag.Role = HttpContext.Session.GetString("Role");
            ViewBag.IsEnrolled = isEnrolled;

            return View(course);
        }

        private List<string> GetLevelList()
        {
            return new List<string> { "Beginner", "Intermediate", "Advanced" };
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin")
            {
                return RedirectToAction("AccessDenied", "Account"); // hoặc về trang Home
            }

            var course = _context.Courses.FirstOrDefault(c => c.Id == id);
            if (course == null) return NotFound();

            ViewBag.Instructors = GetInstructorList();
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

            // ✅ Không cho phép xóa nếu đã có học sinh đăng ký
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
                return RedirectToAction("AccessDenied", "Account"); // hoặc về trang Home
            }
            var course = _context.Courses.FirstOrDefault(c => c.Id == id);
            if (course == null) return NotFound();

            var enrollments = _context.Enrollments
                .Where(e => e.CourseId == id)
                .Include(e => e.Student)
                .ToList();

            ViewBag.CourseName = course.Name;
            return View(enrollments); // 👈 gửi trực tiếp List<Enrollment>
        }



        [HttpPost]
        public IActionResult Enroll(int id)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Student")
            {
                return RedirectToAction("AccessDenied", "Account"); // hoặc về trang Home
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

            // Ghi danh
            student.CurrentBalance -= course.Price;
            course.CurrentSize++;
            _context.Enrollments.Add(new Enrollment { StudentId = student.Id, CourseId = id , EnrollDate = DateTime.Now });
            _context.SaveChanges();

            TempData["EnrollSuccess"] = "Đã ghi danh thành công.";
            return RedirectToAction("Details", new { id });
        }

        [HttpPost]
        public IActionResult Unenroll(int id)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role == null )
            {
                return RedirectToAction("AccessDenied", "Account"); // hoặc về trang Home
            }
            var studentId = HttpContext.Session.GetInt32("UserId");
            if (studentId == null) return RedirectToAction("Login", "Account");

            var enrollment = _context.Enrollments.FirstOrDefault(e => e.CourseId == id && e.StudentId == studentId);
            var student = _context.Students.FirstOrDefault(s => s.Id == studentId);
            var course = _context.Courses.FirstOrDefault(c => c.Id == id);

            if (enrollment == null || student == null || course == null)
            {
                TempData["EnrollError"] = "Không thể hủy ghi danh.";
                return RedirectToAction("Details", new { id });
            }

            if (DateTime.Now >= course.StartDate)
            {
                TempData["EnrollError"] = "Khóa học đã bắt đầu. Không thể hủy ghi danh.";
                return RedirectToAction("Details", new { id });
            }

            // Hủy ghi danh và hoàn lại tiền
            _context.Enrollments.Remove(enrollment);
            student.CurrentBalance += course.Price;
            course.CurrentSize--;
            _context.SaveChanges();

            TempData["EnrollSuccess"] = "Đã hủy ghi danh thành công.";
            return RedirectToAction("Details", new { id });
        }


        [HttpPost]
        public IActionResult Edit(Course course, IFormFile ImageFile)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin")
            {
                return RedirectToAction("AccessDenied", "Account"); // hoặc về trang Home
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

                ViewBag.Instructors = GetInstructorList();
                ViewBag.Categories = GetCategoryList();
                ViewBag.Levels = GetLevelList();
                return View(course);
            }

            var existing = _context.Courses.FirstOrDefault(c => c.Id == course.Id);
            if (existing == null) return NotFound();

            // ✅ Gán lại các giá trị
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

            if (ViewBag.Role == "Admin")
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
                return RedirectToAction("AccessDenied", "Account"); // hoặc về trang Home
            }
            ModelState.Remove("ImageFile");
            ModelState.Remove("DateCreated");
            // Kiểm tra giá trị thủ công
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
                ModelState.AddModelError("Capacity", "Capacity không thể nhỏ hơn 0.");
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
                course.CurrentSize = 0; // ✅ luôn khởi tạo = 0
                course.Duration = (course.EndDate - course.StartDate).Days;

                // Xử lý ảnh
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

            // Nếu có lỗi thì nạp lại dropdown
            ViewBag.Instructors = GetInstructorList();
            ViewBag.Categories = GetCategoryList();
            ViewBag.Levels = GetLevelList(); // ✅ Thêm danh sách level cố định

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
                return RedirectToAction("AccessDenied", "Account"); // hoặc về trang Home
            }
            int? studentId = HttpContext.Session.GetInt32("UserId");
            if (studentId == null) return RedirectToAction("Login", "Account");

            //Trả về Enrollment của UserID hiện tại , sắp xếp giảm theo Enroll Date
            var enrollments = _context.Enrollments
                .Where(e => e.StudentId == studentId)
                .Include(e => e.Course)
                .OrderByDescending(e => e.EnrollDate)
                .ToList();

            // Dictionary: CourseId => EnrollDate , Lưu lại các ID_Course và EnrollDate để hiển thị bên VIEW
            var enrollDates = enrollments.ToDictionary(
                e => e.Course.Id,
                e => e.EnrollDate.ToString("HH:mm:ss dd/MM/yyyy")
            );
            ViewBag.CourseEnrollDates = enrollDates;

            // ✅ Truy xuất Course  ra từ Enrollment
            var courseQuery = enrollments.Select(e => e.Course).AsQueryable();

            // 🔍 Áp dụng filter
            if (!string.IsNullOrEmpty(name))
                courseQuery = courseQuery.Where(c => c.Name.ToLower().Contains(name.ToLower()));

            if (startDate.HasValue)
                courseQuery = courseQuery.Where(c => c.StartDate >= startDate.Value);

            if (!string.IsNullOrEmpty(instructor))
                courseQuery = courseQuery.Where(c => c.InstructorName == instructor);

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

            // ✅ Gán lại dropdown & filter ViewBag
            ViewBag.MyCourseMode = true;
            ViewBag.Instructors = GetInstructorList();
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




        private List<string> GetInstructorList()
        {
            return new List<string>
            {
                "Nguyễn Văn Toàn",
                "Trần Thị Phượng",
                "Robert Junior",
                "Alex Michael",
                "Isagi Yoichi", 
                "Nagi Senshiroi"
            };
        }

        private List<string> GetCategoryList()
        {
            return new List<string>
            {
                "IT",
                "Design",
                "Marketing",
                "Finance",
                "Literature",
            };
        }
    }
}
