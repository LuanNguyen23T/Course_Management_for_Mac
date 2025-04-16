using System.ComponentModel.DataAnnotations;

namespace CourseManagement.ViewModels.Users
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập Mã học viên")]
        public string? MaHocVien { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Mật khẩu")]
        public string? MatKhau { get; set; }

        [Required(ErrorMessage = "Vui lòng xác nhận lại Mật khẩu")]
        public string? XacNhanMatKhau { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Email")]
        public string? Email { get; set; }

    }
}
