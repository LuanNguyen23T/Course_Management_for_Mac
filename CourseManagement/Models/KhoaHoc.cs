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
    }
}
