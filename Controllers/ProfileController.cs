using HCMSIU_SSPS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HCMSIU_SSPS.Controllers
{
    public class ProfileController : Controller
    {
        private readonly HcmsiuSspsContext _context;

        public ProfileController(HcmsiuSspsContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Details(string? username)
        {
            if (username == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Where(m => m.UserName == username)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

    }
}
