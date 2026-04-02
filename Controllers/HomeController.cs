using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using SureAdmitCore.Areas.Admin.Models;
using SureAdmitCore.Data;
using SureAdmitCore.Models;
using System.Data;
using System.Diagnostics;
using Stripe;
using Stripe.Checkout;

namespace SureAdmitCore.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IDbLayer _dbLayer; 
        public HomeController(ILogger<HomeController> logger, IDbLayer dbLayer)
        {
            _logger = logger;
            _dbLayer = dbLayer;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult sureadmitessentialspackage()
        {
            return View();
        }
        public IActionResult Scholaria()
        {
            return View();
        }

        //public async Task<IActionResult> Scholaria()
        //{
        //    // Fetch all active courses
        //    var parameters = new SqlParameter[]
        //    {
        //new SqlParameter("@Action", "Select"),
        //new SqlParameter("@FilterVal", DBNull.Value) // no search
        //    };

        //    DataTable dt = await _dbLayer.ExecuteSPAsync("sp_ManageCourse", parameters);

        //    var courses = dt.AsEnumerable()
        //        .Where(r => r["IsActive"] != DBNull.Value && Convert.ToBoolean(r["IsActive"]))
        //        .Select(r => new Course
        //        {
        //            CourseId = Convert.ToInt32(r["CourseId"]),
        //            CourseName = r["CourseName"]?.ToString() ?? string.Empty,
        //            CourseImgPath = r["CourseImgPath"]?.ToString() ?? "/images/no-image.png",
        //            CoursePrice = r["CoursePrice"]?.ToString() ?? "0"
        //        }).ToList();

        //    return View(courses);
        //}
        public IActionResult gradedgefundingtm()
        {
            return View();
        }
        public IActionResult gradedgevalue()
        {
            return View();
        }
        public IActionResult feespaymentstructure()
        {
            return View();
        }

        // Add course ID to cart (cookie-based)
        public IActionResult AddToCart(int courseId)
        {
            var cart = Request.Cookies.GetObject<List<int>>("Cart") ?? new List<int>();
            if (!cart.Contains(courseId))
                cart.Add(courseId);

            Response.Cookies.SetObject("Cart", cart); // Update cookie
            return RedirectToAction("CourseCart");
        }

        // Remove course from cart
        public IActionResult RemoveFromCart(int courseId)
        {
            var cart = Request.Cookies.GetObject<List<int>>("Cart") ?? new List<int>();
            cart.Remove(courseId);
            Response.Cookies.SetObject("Cart", cart);
            return RedirectToAction("CourseCart");
        }

        public async Task<IActionResult> CourseCart()
        {
            // Get cart IDs from cookies
            var cartIds = Request.Cookies.GetObject<List<int>>("Cart") ?? new List<int>();
            if (!cartIds.Any())
                return View(new List<Course>());

            // Fetch all courses from DB
            var parameters = new SqlParameter[]
            {
        new SqlParameter("@Action", "Select"),
        new SqlParameter("@FilterVal", DBNull.Value)
            };

            DataTable dt = await _dbLayer.ExecuteSPAsync("sp_ManageCourse", parameters);

            // Filter courses based on cart IDs
            var courses = dt.AsEnumerable()
                .Where(r => cartIds.Contains(Convert.ToInt32(r["CourseId"])))
                .Select(r =>
                {
                    string priceStr = r["CoursePrice"]?.ToString() ?? "0"; // e.g. "$4,470"

                    // Extract currency symbol (first non-digit character)
                    string symbol = new string(priceStr.TakeWhile(ch => !char.IsDigit(ch) && ch != ',' && ch != '.').ToArray());

                    return new Course
                    {
                        CourseId = Convert.ToInt32(r["CourseId"]),
                        CourseName = r["CourseName"]?.ToString() ?? string.Empty,
                        CourseImgPath = r["CourseImgPath"]?.ToString() ?? "/images/no-image.png",
                        CoursePrice = priceStr,       // Keep original with symbol
                        CurrencySymbol = symbol       // store symbol separately
                    };
                }).ToList();

            return View(courses);
        }

        public async Task<IActionResult> Checkout()
        {
            var model = new CourseCheckoutModel();

            // Get cart IDs from cookie
            var cartIds = Request.Cookies.GetObject<List<int>>("Cart") ?? new List<int>();

            if (cartIds.Any())
            {
                var idsParam = string.Join(",", cartIds);
                var dtCourses = await _dbLayer.ExecuteSPAsync("sp_ManageCourse", new SqlParameter[]
                {
            new SqlParameter("@Action", "SelectByCartCourseId"),
            new SqlParameter("@CartCourseId", idsParam)
                });

                var courses = dtCourses.AsEnumerable()
                    .Select(r =>
                    {
                        var priceStr = r["CoursePrice"]?.ToString() ?? "0";
                        // Extract currency symbol (first non-digit character)
                        string currencySymbol = new string(priceStr.TakeWhile(ch => !char.IsDigit(ch) && ch != ',' && ch != '.').ToArray());

                        // Clean number for calculations
                        var cleanPriceStr = priceStr.Replace(currencySymbol, "").Replace(",", "").Trim();
                        decimal.TryParse(cleanPriceStr, out var price);

                        return new Coursecheckout
                        {
                            CourseId = Convert.ToInt32(r["CourseId"]),
                            CourseName = r["CourseName"]?.ToString() ?? string.Empty,
                            CourseImgPath = r["CourseImgPath"]?.ToString() ?? "/images/no-image.png",
                            CoursePrice = price,
                            CoursePriceDisplay = priceStr // keep original string for display
                        };
                    }).ToList();

                model.CartCourses = courses;
                model.Subtotal = courses.Sum(c => c.CoursePrice);
                model.GST = Math.Round(model.Subtotal * 0.18m, 2);
                model.Total = model.Subtotal + model.GST;
            }
            else
            {
                model.CartCourses = new List<Coursecheckout>();
                model.Subtotal = 0;
                model.GST = 0;
                model.Total = 0;
            }

            // Fetch countries
            var dtCountries = await _dbLayer.ExecuteSPAsync("sp_GetCountries", new SqlParameter[]
            {
        new SqlParameter("@Action", "Select")
            });

            model.CountryList = dtCountries.AsEnumerable()
                .Select(r => new SelectListItem
                {
                    Text = r["CountryName"]?.ToString() ?? "",
                    Value = r["CountryId"]?.ToString() ?? ""
                }).ToList();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(CourseCheckoutModel model)
        {
            var cartIds = Request.Cookies.GetObject<List<int>>("Cart") ?? new List<int>();
            if (!cartIds.Any())
                return RedirectToAction("CourseCart");

            var dtCourses = await _dbLayer.ExecuteSPAsync("sp_ManageCourse", new SqlParameter[]
            {
                new SqlParameter("@Action", "Select")
            });

            var courses = dtCourses.AsEnumerable()
                .Where(r => cartIds.Contains(Convert.ToInt32(r["CourseId"])))
                .Select(r =>
                {
                    var priceStr = r["CoursePrice"]?.ToString() ?? "0";
                    string currencySymbol = new string(priceStr.TakeWhile(ch => !char.IsDigit(ch) && ch != ',' && ch != '.').ToArray());
                    var cleanPriceStr = priceStr.Replace(currencySymbol, "").Replace(",", "").Trim();
                    decimal.TryParse(cleanPriceStr, out var price);

                    return new Coursecheckout
                    {
                        CourseId = Convert.ToInt32(r["CourseId"]),
                        CourseName = r["CourseName"]?.ToString() ?? string.Empty,
                        CoursePrice = price,
                        CoursePriceDisplay = priceStr
                    };
                }).ToList();

            decimal subtotal = model.Subtotal;
            decimal gst = model.GST;
            decimal total = model.Total;
            string currencySymbol = model.SelectedCurrency == "INR" ? "₹" : "$";

            var outputParam = new SqlParameter("@OutputId", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };

            await _dbLayer.ExecuteSPAsync("sp_ManageCourseApplication", new SqlParameter[]
            {
                new SqlParameter("@Action", "Insert"),
                new SqlParameter("@Name", model.Name),
                new SqlParameter("@Email", model.Email),
                new SqlParameter("@Phone", model.Phone),
                new SqlParameter("@BachelorCGPA", model.BachelorCGPA ?? (object)DBNull.Value),
                new SqlParameter("@MasterCGPA", model.MasterCGPA ?? (object)DBNull.Value),
                new SqlParameter("@GREVerbal", model.GREVerbal ?? (object)DBNull.Value),
                new SqlParameter("@GREQuant", model.GREQuant ?? (object)DBNull.Value),
                new SqlParameter("@Message", model.Message ?? (object)DBNull.Value),
                new SqlParameter("@CartCourseIds", string.Join(",", cartIds)),
                new SqlParameter("@BaseAmount", subtotal),
                new SqlParameter("@GSTAmount", gst),
                new SqlParameter("@TotalAmount", total),
                outputParam
            });

            int applicationId = (outputParam.Value != DBNull.Value) ? (int)outputParam.Value : 0;
            if (applicationId == 0)
                return BadRequest("Unable to save application.");
            TempData["Currency"] = model.SelectedCurrency;
            TempData["ApplicationId"] = applicationId; 
            TempData["TotalAmount"] = total.ToString("F2");
            TempData["CurrencySymbol"] = courses.FirstOrDefault()?.CoursePriceDisplay?.FirstOrDefault() == '₹' ? "₹" : "$";

            return RedirectToAction("PaymentGateway");
        }

        public IActionResult PaymentGateway()
        {
            var applicationId = (int?)TempData["ApplicationId"] ?? 0;

            var totalAmountStr = TempData["TotalAmount"]?.ToString() ?? "0";
            decimal totalAmount = decimal.Parse(totalAmountStr);

            if (applicationId == 0 || totalAmount == 0)
                return RedirectToAction("Checkout");

            // Razorpay keys
            ViewBag.RazorpayKey = "rzp_live_S10LtkcbKIWzQW";
            ViewBag.Amount = (int)(totalAmount * 100); // convert to paise
            ViewBag.ApplicationId = applicationId;
            ViewBag.TotalAmount = totalAmount;
            ViewBag.Currency = TempData["Currency"]?.ToString() ?? "INR";
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> PaymentResponse(string applicationid, string status, string paymentId)
        {
            int appId = 0;
            int.TryParse(applicationid, out appId);

            if (appId == 0)
                return RedirectToAction("Checkout");

            string paymentStatus = status == "success" ? "Success" : "Failed";

            // ✅ FIX: correct amount
            decimal amount = 0;
            decimal.TryParse(TempData["TotalAmount"]?.ToString(), out amount);

            // ✅ FIX: currency
            string currency = TempData["Currency"]?.ToString() ?? "INR";

            // ✅ keep TempData safe
            TempData.Keep("TotalAmount");
            TempData.Keep("Currency");

            string gatewayResponse = $"{paymentId}";

            await _dbLayer.ExecuteSPAsync("sp_UpdatePaymentStatus", new SqlParameter[]
            {
        new SqlParameter("@ApplicationId", appId),
        new SqlParameter("@PaymentStatus", paymentStatus),
        new SqlParameter("@PaymentRefNo", paymentId ?? (object)DBNull.Value),
        new SqlParameter("@Amount", amount),
        new SqlParameter("@Currency", currency),
        new SqlParameter("@Gateway", "Razorpay"),
        new SqlParameter("@GatewayResponse", gatewayResponse)
            });

            ViewBag.Message = status == "success" ? "Payment Successful!" : "Payment Failed!";
            ViewBag.PaymentId = paymentId;

            return View();
        }



    }
}
