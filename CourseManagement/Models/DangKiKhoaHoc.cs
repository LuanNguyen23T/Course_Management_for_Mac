using System.ComponentModel.DataAnnotations;

namespace CourseManagement.Models
{
    public class DangKiKhoaHoc
    {
        public string MaHocVien { get; set; }
        public string MaKhoaHoc { get; set; }
        public DateTime NgayDangKy { get; set; }
    }
}
