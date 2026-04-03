
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
        public async Task<IActionResult> CourseList(string? search = null, int? status = null)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
        new SqlParameter("@Action", "Select"),
        new SqlParameter("@FilterVal", string.IsNullOrWhiteSpace(search) ? DBNull.Value : search.Trim()),
        new SqlParameter("@Status", status.HasValue ? status.Value : (object)DBNull.Value)
            };

            DataTable dt = await _dbLayer.ExecuteSPAsync("sp_ManageCourse", parameters);

            var courses = dt.AsEnumerable().Select(r => new Course
            {
                CourseId = r["CourseId"] != DBNull.Value ? Convert.ToInt32(r["CourseId"]) : 0,
                CourseName = r["CourseName"]?.ToString() ?? string.Empty,
                CourseImgPath = r["CourseImgPath"]?.ToString() ?? string.Empty,
                CoursePrice = r["CoursePrice"]?.ToString() ?? string.Empty,
                CourseDescription = r["CourseDescription"]?.ToString() ?? string.Empty,
                IsActive = r["IsActive"] != DBNull.Value && Convert.ToBoolean(r["IsActive"])
            }).ToList();

            ViewData["Search"] = search;
            ViewData["Status"] = status;

            return View(courses);
        }

        [HttpGet]
        public async Task<IActionResult> AddCourse(int? id)
        {
            if (id == null)
                return View(new Course { IsActive = true });

            SqlParameter[] parameters = new SqlParameter[]
            {
        new SqlParameter("@Action", "SelectById"),
        new SqlParameter("@CourseId", id)
            };

            DataTable dt = await _dbLayer.ExecuteSPAsync("sp_ManageCourse", parameters);

            if (dt.Rows.Count == 0) return NotFound();

            var course = new Course
            {
                CourseId = Convert.ToInt32(dt.Rows[0]["CourseId"]),
                CourseName = dt.Rows[0]["CourseName"]?.ToString() ?? string.Empty,
                CourseImgPath = dt.Rows[0]["CourseImgPath"]?.ToString() ?? string.Empty,
                CoursePrice = dt.Rows[0]["CoursePrice"]?.ToString() ?? string.Empty,
                CourseDescription = dt.Rows[0]["CourseDescription"]?.ToString() ?? string.Empty 
            };

            return View(course);
        }

        [HttpPost]
        public async Task<IActionResult> AddCourse(Course model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Message"] = "Please fill all required fields.";
                TempData["MessageType"] = "error";
                return View(model);
            }

            try
            {
                // ✅ IMAGE UPLOAD ONLY IF NEW FILE IS SELECTED
                if (model.CourseImg != null && model.CourseImg.Length > 0)
                {
                    string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.CourseImg.FileName);
                    string filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.CourseImg.CopyToAsync(stream);
                    }

                    // Set new image path
                    model.CourseImgPath = "/uploads/" + fileName;
                }
                else
                {
                    // ✅ If no new image selected, keep existing path from DB
                    SqlParameter[] selectParams = new SqlParameter[]
                    {
                new SqlParameter("@Action", "SelectById"),
                new SqlParameter("@CourseId", model.CourseId)
                    };

                    DataTable dt = await _dbLayer.ExecuteSPAsync("sp_ManageCourse", selectParams);

                    if (dt.Rows.Count > 0)
                    {
                        model.CourseImgPath = dt.Rows[0]["CourseImgPath"]?.ToString() ?? string.Empty;
                    }
                }

                string action = model.CourseId > 0 ? "Update" : "Insert";

                SqlParameter[] parameters = new SqlParameter[]
                {
            new SqlParameter("@Action", action),
            new SqlParameter("@CourseId", model.CourseId),
            new SqlParameter("@CourseName", model.CourseName),
            new SqlParameter("@CourseImgPath", model.CourseImgPath ?? (object)DBNull.Value),
            new SqlParameter("@CourseDescription", model.CourseDescription ?? (object)DBNull.Value),
            new SqlParameter("@CoursePrice", model.CoursePrice),
            new SqlParameter("@IsActive", model.IsActive)
                };

                await _dbLayer.ExecuteSPAsync("sp_ManageCourse", parameters);

                TempData["Message"] = action == "Insert"
                    ? "Course added successfully!"
                    : "Course updated successfully!";
                TempData["MessageType"] = "success";
            }
            catch (SqlException ex)
            {
                TempData["Message"] = ex.Message;
                TempData["MessageType"] = "error";
                return View(model);
            }
            catch (Exception)
            {
                TempData["Message"] = "Something went wrong.";
                TempData["MessageType"] = "error";
                return View(model);
            }

            return RedirectToAction("CourseList");
        }


        [HttpPost]
        public async Task<IActionResult> ToggleCourseStatus(int id)
        {
            try
            {
                await _dbLayer.ExecuteSPAsync("sp_ManageCourse", new[]
                {
            new SqlParameter("@Action", "ToggleStatus"),
            new SqlParameter("@CourseId", id)
        });

                TempData["Message"] = "Course status updated successfully!";
                TempData["MessageType"] = "success";
            }
            catch
            {
                TempData["Message"] = "Unable to update course status.";
                TempData["MessageType"] = "error";
            }

            return RedirectToAction("CourseList");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            try
            {
                await _dbLayer.ExecuteSPAsync("sp_ManageCourse", new[]
                {
            new SqlParameter("@Action", "Delete"),
            new SqlParameter("@CourseId", id)
        });

                TempData["Message"] = "Course deleted successfully!";
                TempData["MessageType"] = "success";
            }
            catch
            {
                TempData["Message"] = "Unable to delete course.";
                TempData["MessageType"] = "error";
            }

            return RedirectToAction("CourseList");
        }


        [HttpPost]
        public async Task<IActionResult> DeleteSelectedCourses(int[] selectedIds)
        {
            try
            {
                if (selectedIds == null || selectedIds.Length == 0)
                {
                    TempData["Message"] = "Please select at least one course.";
                    TempData["MessageType"] = "warning";
                    return RedirectToAction("CourseList");
                }

                foreach (var id in selectedIds)
                {
                    await _dbLayer.ExecuteSPAsync("sp_ManageCourse", new SqlParameter[]
                    {
                new SqlParameter("@Action", "Delete"),
                new SqlParameter("@CourseId", id)
                    });
                }

                TempData["Message"] = $"{selectedIds.Length} course(s) deleted successfully!";
                TempData["MessageType"] = "success";
            }
            catch
            {
                TempData["Message"] = "Unable to delete selected courses.";
                TempData["MessageType"] = "error";
            }

            return RedirectToAction("CourseList");
        }





    }
}
