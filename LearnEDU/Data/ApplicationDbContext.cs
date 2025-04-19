using Microsoft.EntityFrameworkCore;
using LearnEDU.Models;

namespace LearnEDU.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Student> Students { get; set; }
        public DbSet<Course> Courses { get; set; }

        public DbSet<Enrollment> Enrollments { get; set; }
    }
}
