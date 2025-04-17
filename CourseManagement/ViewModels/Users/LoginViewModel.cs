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

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự.")]
        [DataType(DataType.Password)]
        public string? NewPassword { get; set; }
    }
}
