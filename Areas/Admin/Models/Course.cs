using System.ComponentModel.DataAnnotations;

namespace SureAdmitCore.Areas.Admin.Models
{
    public class Course
    {
        public int CourseId { get; set; }

        [Required]
        [Display(Name = "Course Name")]
        public string CourseName { get; set; }=string.Empty;

        [Display(Name = "Course Image")]
        public IFormFile? CourseImg { get; set; }
        public string CourseImgPath { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Course Price")]
        public string CoursePrice { get; set; } = string.Empty;

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; }
 
     public string? CurrencySymbol { get; set; } // e.g. "$"
        
    }
}
