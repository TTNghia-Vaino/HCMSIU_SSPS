using Microsoft.AspNetCore.Mvc;
using HCMSIU_SSPS.Models; // Sửa lại namespace nếu cần
using System.Linq;

namespace HCMSIU_SSPS.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HomeController : Controller
    {
        private readonly HcmsiuSspsContext _context;

        public HomeController(HcmsiuSspsContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var totalUser = _context.Users.Count();
            var totalPrinter = _context.Printers.Count(p => p.IsEnable == 1);
            var totalPrintJob = _context.PrintJobs.Count(j => j.Status == 0);

            ViewBag.TotalUser = totalUser;
            ViewBag.TotalPrinter = totalPrinter;
            ViewBag.TotalPrintJob = totalPrintJob;

            return View();
        }
    }
}
