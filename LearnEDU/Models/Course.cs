using System;
using System.ComponentModel.DataAnnotations;

namespace LearnEDU.Models
{
    public class Course
    {
        public int Id { get; set; }

        [Display(Name = "Course Name")]
        public string Name { get; set; }

        [Display(Name = "Description")]
        public string? Description { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }

        [Display(Name = "Capacity")]
        public int Capacity { get; set; }

        [Display(Name = "Instructor")]
        public string InstructorName { get; set; }

        [Display(Name = "Image")]
        public string? ImageUrl { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Created Date")]
        public DateTime DateCreated { get; set; }

        [Display(Name = "Category")]
        public string Category { get; set; }

        public int CurrentSize { get; set; } = 0;

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Giá phải lớn hơn hoặc bằng 0")]
        public int Price { get; set; }

        [Required]
        public string Level { get; set; }

        public string Content { get; set; }

        public int Duration { get; set; } // sẽ được gán tự động = (EndDate - StartDate).Days
    }
}

