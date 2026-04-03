using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SureAdmitCore.Areas.Admin.Models;
using SureAdmitCore.Data;
using System.Data;

namespace SureAdmitCore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class ReportController : BaseController
    {
        private readonly IDbLayer _dbLayer;

        public ReportController(IDbLayer dbLayer)
        {
            _dbLayer = dbLayer;
        }

        // GET: List of applied courses
        [HttpGet]
        public async Task<IActionResult> AppliedCourseDetails(string? search = null, int? status = null)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
        new SqlParameter("@Action", "Select"),
        new SqlParameter("@FilterVal", string.IsNullOrWhiteSpace(search) ? DBNull.Value : search.Trim()),
        new SqlParameter("@Status", status.HasValue ? status.Value : (object)DBNull.Value)
            };

            DataTable dt = await _dbLayer.ExecuteSPAsync("sp_GetAppliedCourseDetails", parameters);

            var courses = dt.AsEnumerable().Select(r => new AppliedCourse
            {
                ApplicationId = r["ApplicationId"] != DBNull.Value ? Convert.ToInt32(r["ApplicationId"]) : 0,
                Name = r["Name"]?.ToString() ?? string.Empty,
                Email = r["Email"]?.ToString() ?? string.Empty,
                Phone = r["Phone"]?.ToString() ?? string.Empty,
                Address = r["Address"]?.ToString() ?? string.Empty,
                PinCode = r["PinCode"]?.ToString() ?? string.Empty,
                City = r["City"]?.ToString() ?? string.Empty,
                IsAgreementAccepted = r["IsAgreementAccepted"] != DBNull.Value && Convert.ToBoolean(r["IsAgreementAccepted"]),
                Country = r["CountryName"]?.ToString() ?? string.Empty,
                BachelorCGPA = r["BachelorCGPA"]?.ToString() ?? string.Empty,
                MasterCGPA = r["MasterCGPA"]?.ToString() ?? string.Empty,
                GREVerbal = r["GREVerbal"]?.ToString() ?? string.Empty,
                GREQuant = r["GREQuant"]?.ToString() ?? string.Empty,
                Message = r["Message"]?.ToString() ?? string.Empty,
                CourseNames = r["CourseNames"]?.ToString() ?? string.Empty,
                BaseAmount = r["BaseAmount"] != DBNull.Value ? Convert.ToDecimal(r["BaseAmount"]) : 0,
                GSTAmount = r["GSTAmount"] != DBNull.Value ? Convert.ToDecimal(r["GSTAmount"]) : 0,
                TotalAmount = r["TotalAmount"] != DBNull.Value ? Convert.ToDecimal(r["TotalAmount"]) : 0,
                PaymentStatus = r["PaymentStatus"]?.ToString() ?? "Pending",
                PaymentRefNo = r["PaymentRefNo"]?.ToString() ?? string.Empty,
                BookingId = r["BookingId"]?.ToString() ?? string.Empty,
                PaymentGateway = r["Gateway"]?.ToString() ?? string.Empty,
                PaidAmount = r["PaidAmount"] != DBNull.Value ? Convert.ToDecimal(r["PaidAmount"]) : 0,
                Currency = r["Currency"]?.ToString() ?? "INR",
                PaymentLogStatus = r["PaymentLogStatus"]?.ToString() ?? string.Empty,
                PaymentDate = r["PaymentDate"] != DBNull.Value ? Convert.ToDateTime(r["PaymentDate"]) : (DateTime?)null,
                IsActive = r["IsActive"] != DBNull.Value && Convert.ToBoolean(r["IsActive"]),
                CreatedOn = r["CreatedOn"] != DBNull.Value ? Convert.ToDateTime(r["CreatedOn"]) : DateTime.Now
            }).ToList();

            ViewData["Search"] = search;
            ViewData["Status"] = status;

            return View(courses);
        }

        // POST: Delete a single application
        [HttpPost]
        public async Task<IActionResult> DeleteApplication(int id)
        {
            try
            {
                await _dbLayer.ExecuteSPAsync("sp_GetAppliedCourseDetails", new[]
                {
                    new SqlParameter("@Action", "Delete"),
                    new SqlParameter("@ApplicationId", id)
                });

                TempData["Message"] = "Application deleted successfully!";
                TempData["MessageType"] = "success";
            }
            catch
            {
                TempData["Message"] = "Unable to delete application.";
                TempData["MessageType"] = "error";
            }

            return RedirectToAction("AppliedCourseDetails");
        }

        // POST: Bulk delete selected applications
        [HttpPost]
        public async Task<IActionResult> DeleteSelectedApplications(int[] selectedIds)
        {
            try
            {
                if (selectedIds == null || selectedIds.Length == 0)
                {
                    TempData["Message"] = "Please select at least one application.";
                    TempData["MessageType"] = "warning";
                    return RedirectToAction("AppliedCourseDetails");
                }

                foreach (var id in selectedIds)
                {
                    await _dbLayer.ExecuteSPAsync("sp_GetAppliedCourseDetails", new[]
                    {
                        new SqlParameter("@Action", "Delete"),
                        new SqlParameter("@ApplicationId", id)
                    });
                }

                TempData["Message"] = $"{selectedIds.Length} application(s) deleted successfully!";
                TempData["MessageType"] = "success";
            }
            catch
            {
                TempData["Message"] = "Unable to delete selected applications.";
                TempData["MessageType"] = "error";
            }

            return RedirectToAction("AppliedCourseDetails");
        }












    }
}
