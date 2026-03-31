using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc; 
using Microsoft.Data.SqlClient;
using SureAdmitCore.Data;
using SureAdmitCore.Models;
using System.Data;
using System.Security.Claims;

namespace SureAdmitCore.Controllers
{
    public class AccountController : Controller
    {
        private readonly IDbLayer _dbLayer;

        public AccountController(IDbLayer dbLayer)
        {
            _dbLayer = dbLayer;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                SqlParameter[] parameters =
                {
            new SqlParameter("@UserName", model.Username),
            new SqlParameter("@Password", model.Password)
        };

                DataTable dt = await _dbLayer.ExecuteSPAsync("sp_LoginUser", parameters);

                if (dt == null || dt.Rows.Count == 0)
                {
                    TempData["Message"] = "Something went wrong. Please try again.";
                    TempData["MessageType"] = "error";
                    return View(model);
                }

                DataRow row = dt.Rows[0];
                int loginSuccess = Convert.ToInt32(row["LoginSuccess"]);

                // ❌ LOGIN FAILED
                if (loginSuccess == 0)
                {
                    TempData["Message"] = row["Message"]?.ToString() ?? "Login failed";
                    TempData["MessageType"] = "error";
                    return View(model);
                }

                // ✅ LOGIN SUCCESS

                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, row["UserId"].ToString()),
            new Claim(ClaimTypes.Name, row["UserName"].ToString()),
            new Claim(ClaimTypes.Role, row["UserType"].ToString())
        };

                var identity = new ClaimsIdentity(claims, "MyCookieAuth");
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync("MyCookieAuth", principal);

                TempData["Message"] = "Login successfully!";
                TempData["MessageType"] = "success";

                string userType = row["UserType"].ToString().Trim();

                // 🔀 Role-based redirect
                if (userType.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });

                if (userType.Equals("User", StringComparison.OrdinalIgnoreCase))
                    return RedirectToAction("Index", "Home");

                // Default fallback
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Error: " + ex.Message;
                TempData["MessageType"] = "error";
                return View(model);
            }
        }

        [HttpPost]
            public IActionResult Logout()
            { 
                HttpContext.Session.Clear(); 
                // await HttpContext.SignOutAsync();

                return RedirectToAction("Login", "Account");
            }
       
   

}
}
