using System.ComponentModel.DataAnnotations;

namespace LearnEDU.Models
{
    public class Student
    {
        public int Id { get; set; }

        [Required]
        public string Username { get; set; }

        [Required, DataType(DataType.Password)]
        public string Password { get; set; }

        [Required, Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required, Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        public DateTime DateOfBirth { get; set; }

        public string Phone { get; set; }

        public string Address { get; set; }

        public string Gender { get; set; }

        public string Education { get; set; }

        public string? Image { get; set; }

        [DataType(DataType.Date)]
        public DateTime DateRegister { get; set; }

        [Required]
        public string Role { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Ngân sách không được âm")]
        [Display(Name = "Ngân sách hiện tại (VNĐ)")]
        public int CurrentBalance { get; set; }
    }
}
