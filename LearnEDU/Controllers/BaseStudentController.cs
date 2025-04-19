using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using LearnEDU.Models;
using LearnEDU.Data;

public class BaseStudentController : Controller
{
    protected readonly ApplicationDbContext _context;

    public BaseStudentController(ApplicationDbContext context)
    {
        _context = context;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var role = HttpContext.Session.GetString("Role");

        if (role == "Student")
        {
            var id = HttpContext.Session.GetInt32("UserId");
            var student = _context.Students.FirstOrDefault(s => s.Id == id);

            if (student != null)
            {
                ViewBag.CurrentBalance = student.CurrentBalance;
            }
        }

        base.OnActionExecuting(context);
    }
}
