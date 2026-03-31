using System.ComponentModel.DataAnnotations;

namespace SureAdmitCore.Areas.Admin.Models
{
    public class Department
    {
        public int DepartmentId { get; set; }

        [Required]
        [Display(Name = "Department Name")]
        public string DepartmentName { get; set; }

        public bool IsActive { get; set; }
    }
}
