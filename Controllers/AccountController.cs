using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc; 
using Microsoft.Data.SqlClient;
using SureAdmitCore.Data;
using SureAdmitCore.Models;
using System.Data;
using System.Net;
using System.Net.Mail;
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
            Response.Cookies.Delete(".AspNetCore.Cookies");  
            return RedirectToAction("Login", "Account");
        }

        // GET: Account/ForgetPassword
        [HttpGet]
        public IActionResult ForgetPassword()
        {
            return View();
        }

        // POST: Account/ForgetPassword
        [HttpPost]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordViewModel model, string actionType)
        {
            // Remove fields from ModelState to allow partial validation
            ModelState.Remove("Email");
            ModelState.Remove("OTP");
            ModelState.Remove("NewPassword");
            ModelState.Remove("ConfirmPassword");

            if (!ModelState.IsValid)
            {
                TempData["Message"] = "Please fill all required fields.";
                TempData["MessageType"] = "error";
                return View(model);
            }

            if (actionType == "SendOTP")
            {
                // Check if email exists in DB
                SqlParameter[] parameters = new SqlParameter[] { new SqlParameter("@Email", model.Email) };
                DataTable dt = await _dbLayer.ExecuteSPAsync("sp_CheckUserEmail", parameters);

                if (dt.Rows.Count == 0)
                {
                    TempData["Message"] = "Email is not registered.";
                    TempData["MessageType"] = "error";
                    return View(model);
                }

                // Generate OTP
                var otp = new Random().Next(100000, 999999).ToString();
                // Set OTP in cookies
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,                  // JS se access na ho
                    Expires = DateTime.Now.AddMinutes(1) // 1 minute expiry
                }; 
                // OTP
                HttpContext.Response.Cookies.Append("OTP", otp, cookieOptions);

                // Email
                HttpContext.Response.Cookies.Append("OTPEmail", model.Email, cookieOptions);

                // OTP expiry (optional, agar aap separate expiry chahte ho)
                HttpContext.Response.Cookies.Append("OTPExpiry", DateTime.Now.AddMinutes(1).ToString("o"), cookieOptions);

                // Send OTP via email
                SendOTPEmail(model.Email, otp);

                TempData["Message"] = "OTP sent to your email (valid for 1 minute).";
                TempData["MessageType"] = "success";

                ViewData["IsOTPSent"] = true;
                ViewData["IsOTPExpired"] = null; // OTP just sent, not expired
                return View(model);
            }

            if (actionType == "ResetPassword")
            {
                // Read OTP from cookie
                string cookieOtp = HttpContext.Request.Cookies["OTP"];
                string cookieEmail = HttpContext.Request.Cookies["OTPEmail"];

                // Optional: Read expiry if stored
                string cookieExpiryStr = HttpContext.Request.Cookies["OTPExpiry"];
                DateTime otpExpiry;
                if (!string.IsNullOrEmpty(cookieExpiryStr) && DateTime.TryParse(cookieExpiryStr, out otpExpiry))
                {
                    if (DateTime.Now > otpExpiry)
                    {
                        // OTP expired
                        cookieOtp = null;
                        cookieEmail = null;
                    }
                }

                // OTP expired
                if (cookieOtp == null || cookieEmail == null)
                {
                    TempData["Message"] = "OTP expired. Please request a new OTP.";
                    TempData["MessageType"] = "error";

                    // Show "Resend OTP" button instead of OTP/password fields
                    ViewData["IsOTPSent"] = null;
                    ViewData["IsOTPExpired"] = true;
                    return View(model);
                }

                // OTP invalid
                if (model.OTP != cookieOtp || model.Email != cookieEmail)
                {
                    TempData["Message"] = "Invalid OTP.";
                    TempData["MessageType"] = "error";
                    ViewData["IsOTPSent"] = true;
                    ViewData["IsOTPExpired"] = null;
                    return View(model);
                }

                // Password mismatch server-side check
                if (model.NewPassword != model.ConfirmPassword)
                {
                    TempData["Message"] = "Passwords do not match.";
                    TempData["MessageType"] = "error";
                    ViewData["IsOTPSent"] = true;
                    ViewData["IsOTPExpired"] = null;
                    return View(model);
                }

                // Update password in DB
                SqlParameter[] parametersUpdate = new SqlParameter[]
                {
        new SqlParameter("@Email", model.Email),
        new SqlParameter("@Password", model.NewPassword)
                };
                await _dbLayer.ExecuteSPAsync("sp_ResetUserPassword", parametersUpdate);

                // ✅ Clear OTP cookies after successful reset
                Response.Cookies.Delete("OTP");
                Response.Cookies.Delete("OTPEmail");
                Response.Cookies.Delete("OTPExpiry");

                TempData["Message"] = "Password reset successfully. Please login.";
                TempData["MessageType"] = "success";
                return RedirectToAction("Login");
            }
            // fallback
            return View(model);
        }

        private void SendOTPEmail(string email, string otp)
        {
            MailMessage mail = new MailMessage();
            mail.From = new MailAddress("trianglemind14@gmail.com");
            mail.To.Add(email);
            mail.Subject = "Password Reset OTP";
            mail.Body = $"Your OTP for password reset is: <b>{otp}</b>";
            mail.IsBodyHtml = true;

            SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
            smtp.Credentials = new NetworkCredential("trianglemind14@gmail.com", "mgae pptn cmej axlp");
            smtp.EnableSsl = true;
            smtp.Send(mail);
        }


    }
}
