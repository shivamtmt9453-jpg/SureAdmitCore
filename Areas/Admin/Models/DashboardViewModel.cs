using System;

namespace SureAdmitCore.Areas.Admin.Models
{
    public class DashboardViewModel
    {
        // OPD
        public int TotalCourse{ get; set; }
         
        // User
        public string UserRole { get; set; } = string.Empty;
    }
}
