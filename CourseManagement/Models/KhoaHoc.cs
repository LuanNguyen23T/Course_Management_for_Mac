using System.ComponentModel.DataAnnotations;

namespace CourseManagement.Models
{
    public class KhoaHoc
    {
        [Key]
        public string MaKhoaHoc { get; set; }
        public string TenKhoaHoc { get; set; }
        public string GiangVien { get; set; }
        public DateTime ThoiGianKhaiGiang { get; set; }
        public decimal HocPhi { get; set; }
        public int SoLuongSinhVienToiDa { get; set; }
        public string? MoTa { get; set; } // Property for course description
        public string LinhVuc { get; set; } // Property for course field of study
        public string? Url { get; set; } // Property for course image URL
    }
}
