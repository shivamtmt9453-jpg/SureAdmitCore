
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
    public class MasterController : BaseController
    {
        private readonly IDbLayer _dbLayer;

        public MasterController(IDbLayer dbLayer)
        {
            _dbLayer = dbLayer;
        }


        [HttpGet]
        public async Task<IActionResult> DepartmentList(string? search = null, int? status = null)
        {
           
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Action", "Select"),
                new SqlParameter("@FilterVal", string.IsNullOrEmpty(search) ? DBNull.Value : search),
                new SqlParameter("@Status", status.HasValue ? status.Value : (object)DBNull.Value)
            };

            DataTable dt = await _dbLayer.ExecuteSPAsync("sp_ManageDepartment", parameters);

            var departments = dt.AsEnumerable().Select(r => new Department
            {
                DepartmentId = Convert.ToInt32(r["DepartmentId"]),
                DepartmentName = r["DepartmentName"]?.ToString() ?? string.Empty,
                IsActive = Convert.ToBoolean(r["IsActive"])
            }).ToList();

            ViewData["Search"] = search;
            ViewData["Status"] = status;

            return View(departments);
        }


        [HttpGet]
        public async Task<IActionResult> AddDepartment(int? id)
        { 
            if (id == null)
                return View(new Department { IsActive = true });

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Action", "SelectBYId"),
                new SqlParameter("@DepartmentId", id)
            };

            DataTable dt = await _dbLayer.ExecuteSPAsync("sp_ManageDepartment", parameters);

            if (dt.Rows.Count == 0) return NotFound();

            var dept = new Department
            {
                DepartmentId = Convert.ToInt32(dt.Rows[0]["DepartmentId"]),
                DepartmentName = dt.Rows[0]["DepartmentName"]?.ToString() ?? string.Empty,
                IsActive = Convert.ToBoolean(dt.Rows[0]["IsActive"])
            };

            return View(dept);
        }

        [HttpPost]
        public async Task<IActionResult> AddDepartment(Department model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Message"] = "Please fill all required fields.";
                TempData["MessageType"] = "error";
                return View(model);
            }

            try
            {
                string action = model.DepartmentId > 0 ? "Update" : "Insert";

                SqlParameter[] parameters = new SqlParameter[]
                {
            new SqlParameter("@Action", action),
            new SqlParameter("@DepartmentId", model.DepartmentId),
            new SqlParameter("@DepartmentName", model.DepartmentName),
            new SqlParameter("@IsActive", model.IsActive)
                };

                await _dbLayer.ExecuteSPAsync("sp_ManageDepartment", parameters);

                TempData["Message"] = action == "Insert"
                    ? "Department added successfully!"
                    : "Department updated successfully!";

                TempData["MessageType"] = "success";
            }
            catch (SqlException ex)
            {
                // Handle RAISERROR from SP
                TempData["Message"] = ex.Message;
                TempData["MessageType"] = "error";

                return View(model); // Return to the same view with input data
            }
            catch (Exception)
            {
                TempData["Message"] = "Something went wrong. Please try again.";
                TempData["MessageType"] = "error";
                return View(model);
            }

            return RedirectToAction("DepartmentList");
        }


        [HttpPost]
        public async Task<IActionResult> ToggleDepartmentStatus(int id)
        {
            try
            {
                await _dbLayer.ExecuteSPAsync("sp_ManageDepartment", new[]
                {
            new SqlParameter("@Action", "ToggleStatus"),
            new SqlParameter("@DepartmentId", id)
        });

                TempData["Message"] = "Department status updated successfully!";
                TempData["MessageType"] = "success";
            }
            catch
            {
                TempData["Message"] = "Unable to update department status.";
                TempData["MessageType"] = "error";
            }

            return RedirectToAction("DepartmentList");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            try
            {
                await _dbLayer.ExecuteSPAsync("sp_ManageDepartment", new[]
                {
            new SqlParameter("@Action", "Delete"),
            new SqlParameter("@DepartmentId", id)
        });

                TempData["Message"] = "Department deleted successfully!";
                TempData["MessageType"] = "success";
            }
            catch
            {
                TempData["Message"] = "Unable to delete department.";
                TempData["MessageType"] = "error";
            }

            return RedirectToAction("DepartmentList");
        }


        [HttpPost]
        public async Task<IActionResult> DeleteSelectedDepartments(int[] selectedIds)
        {
            try
            {
                if (selectedIds == null || selectedIds.Length == 0)
                {
                    TempData["Message"] = "Please select at least one department.";
                    TempData["MessageType"] = "warning";
                    return RedirectToAction("DepartmentList");
                }

                foreach (var id in selectedIds)
                {
                    await _dbLayer.ExecuteSPAsync("sp_ManageDepartment", new SqlParameter[]
                    {
                new SqlParameter("@Action", "Delete"),
                new SqlParameter("@DepartmentId", id)
                    });
                }

                TempData["Message"] = $"{selectedIds.Length} department(s) deleted successfully!";
                TempData["MessageType"] = "success";
            }
            catch (Exception)
            {
                TempData["Message"] = "Unable to delete selected departments.";
                TempData["MessageType"] = "error";
            }

            return RedirectToAction("DepartmentList");
        }

 

         



    }
}
