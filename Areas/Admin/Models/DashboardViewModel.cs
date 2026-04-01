using System;

namespace SureAdmitCore.Areas.Admin.Models
{
    public class DashboardViewModel
    {
      
        public int TotalCourse{ get; set; }
        public int TotalApliedCourse { get; set; }
         
        // User
        public string UserRole { get; set; } = string.Empty;
    }
}
