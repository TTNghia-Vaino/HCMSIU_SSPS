using Microsoft.AspNetCore.Mvc;

namespace HCMSIU_SSPS.Areas.Admin.Controllers
{
    public class HomeController : Controller
    {
        [Area("Admin")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
