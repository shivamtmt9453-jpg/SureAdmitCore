using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SureAdmitCore.Models;

namespace SureAdmitCore.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult sureadmitessentialspackage()
        {
            return View();
        }
        public IActionResult scholaria()
        {
            return View();
        }
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
        public IActionResult CourseCart()
        {
            return View();
        }


    }
}
