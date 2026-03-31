 
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using SureAdmitCore.Areas.Admin.Models;
using SureAdmitCore.Data;
using System.Collections.Generic; 
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;


namespace SureAdmitCore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class DashboardController : BaseController
    {
        private readonly IDbLayer _dbLayer;

        public DashboardController(IDbLayer dbLayer)
        {
            _dbLayer = dbLayer;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        { 

            var model = new DashboardViewModel();

            //// 🔥 Single SP call for dashboard data
            //var dtDashboard = await _dbLayer.ExecuteSPAsync(
            //    "sp_GetDashboardData",
            //    new[] { new SqlParameter("@Action", "GetDashboardStats") }
            //);

            //if (dtDashboard.Rows.Count > 0)
            //{
            //    var row = dtDashboard.Rows[0];

            //    // OPD Stats
            //    model.OPDTodaysPatients = row["TodaysPatients"] != DBNull.Value ? (int)row["TodaysPatients"] : 0;
            //    model.OPDNewPatients = row["OPDNewPatients"] != DBNull.Value ? (int)row["OPDNewPatients"] : 0;
            //  }

            // User role from session/ViewBag
            model.UserRole = ViewBag.UserRole as string;

            return View(model);
        }


        [HttpGet]
        public IActionResult ChangePassword()
        {
             
            return View(new ChangePasswordViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            int userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            if (userId == 0)
                return RedirectToAction("Login", "Account");

            var resultParam = new SqlParameter("@Result", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };

            await _dbLayer.ExecuteSPAsync(
                "sp_ChangePassword",
                new[]
                {
            new SqlParameter("@UserId", userId),
            new SqlParameter("@CurrentPassword", model.CurrentPassword),
            new SqlParameter("@NewPassword", model.NewPassword),
            resultParam
                }
            );

            int result = Convert.ToInt32(resultParam.Value);

            if (result == -1)
            {
                TempData["Message"] = "Current password is incorrect.";
                TempData["MessageType"] = "error";
                
                return View(model);
            }
            else if (result == 1)
            {
                TempData["Message"] = "Password changed successfully.";
                TempData["MessageType"] = "success"; 
                return RedirectToAction("Index");
            }
            else
            {
                TempData["Message"] = "Server error. Please try again.";
                TempData["MessageType"] = "error"; 
                return View(model);
            }
        }




    }
}
