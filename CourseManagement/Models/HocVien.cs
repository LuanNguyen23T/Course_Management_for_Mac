using System.ComponentModel.DataAnnotations;

namespace CourseManagement.Models
{
    public class HocVien
    {
        [Key]
        public string MaHocVien { get; set; }
        public string? HoTen { get; set; }
        public DateTime? NgaySinh { get; set; }
        public string? SoDienThoai { get; set; }
        public string? Email { get; set; }
        public int Role { get; set; } // 0 = Admin, 1 = User
        public string MatKhau { get; set; }
    }
}