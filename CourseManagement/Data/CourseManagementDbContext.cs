using CourseManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace CourseManagement.Data
{
    public class CourseManagementDbContext : DbContext
    {
        public CourseManagementDbContext(DbContextOptions<CourseManagementDbContext> options) : base(options) { }
        public DbSet<DangKiKhoaHoc> DangKiKhoaHocs { get; set; }
        public DbSet<HocVien> HocViens { get; set; }
        public DbSet<KhoaHoc> KhoaHocs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)  
        {
            modelBuilder.Entity<DangKiKhoaHoc>()
                .HasKey(d => new { d.MaHocVien, d.MaKhoaHoc });

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<KhoaHoc>(entity =>
            {
                entity.Property(e => e.HocPhi)
                      .HasColumnType("decimal(18,2)");
                entity.Property(e => e.MoTa)
                      .HasColumnType("longtext"); // Configure MoTa property
            });
        }

    }
}