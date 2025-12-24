using HCMSIU_SSPS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using HCMSIU_SSPS.ViewModels;

namespace HCMSIU_SSPS.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly HcmsiuSspsContext _context;


        public HomeController(ILogger<HomeController> logger, HcmsiuSspsContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            var username = HttpContext.Session.GetString("UserName");
            var model = new IndexViewModel();

            if (!string.IsNullOrEmpty(username))
            {
                model.FullName = _context.Users
                                    .Where(u => u.UserName == username)
                                    .Select(u => u.FullName)
                                    .FirstOrDefault();
            }

            return View(model);
        }

        public IActionResult GetPageBalance(string userName)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserName == userName);
            if (user != null)
            {
                return Json(new { PageBalance = user.PageBalance });
            }

            return Json(new { PageBalance = "Not found" });
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
