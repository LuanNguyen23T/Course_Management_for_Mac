namespace CourseManagement.ViewModels.Users
{
    public class CourseViewModel
    {
        public string MaKhoaHoc { get; set; } 
        public string TenKhoaHoc { get; set; } 
        public string GiangVien { get; set; } 
        public DateTime ThoiGianKhaiGiang { get; set; } 
        public decimal HocPhi { get; set; } 
        public int SoLuongSinhVienToiDa { get; set; }     //them so lg hien tai sau khi tạo đk

    }
}