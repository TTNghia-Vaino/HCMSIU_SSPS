using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HCMSIU_SSPS.Models;

namespace HCMSIU_SSPS.Controllers
{
    public class PrintJobsController : Controller
    {
        private readonly HcmsiuSspsContext _context;
        private readonly IWebHostEnvironment _webHost;
        public PrintJobsController(HcmsiuSspsContext context, IWebHostEnvironment webHost)
        {
            _context = context;
            _webHost = webHost;
        }

        // GET: PrintJobs
        public async Task<IActionResult> Index(string username)
        {
            var filteredJobs = _context.PrintJobs
                        .Include(p => p.Printer)
                        .Include(p => p.User)
                        .Where(p => p.User.UserName == username);

            return View(await filteredJobs.ToListAsync());
        }

        // GET: PrintJobs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var printJob = await _context.PrintJobs
                .Include(p => p.Printer)
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.PrintJobId == id);
            if (printJob == null)
            {
                return NotFound();
            }

            return View(printJob);
        }

        // GET: PrintJobs/Create
        public IActionResult Create()
        {
            var username = HttpContext.Session.GetString("UserName");
            // Kiểm tra nếu username không null
            if (username != null)
            {
                // Tìm UserId từ bảng Users
                var userId = _context.Users
                                     .Where(u => u.UserName == username)
                                     .Select(u => u.UserId)
                                     .FirstOrDefault();

                // Truyền userId vào ViewBag
                ViewBag.UserId = userId;
            }

            ViewBag.PrinterId = new SelectList(_context.Printers, "PrinterId", "PrinterName");
            return View();
        }

        // POST: PrintJobs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PrintJobId,UserId,PrinterId,PageCount,TotalPages,Copies, IsA3,IsDoubleSided,StartTime,EndTime")] PrintJob printJob, IFormFile file)
        {

            if (ModelState.IsValid)
            {
                if (file != null && file.Length > 0)
                {
                    var FileName = file.FileName;
                    // Đường dẫn lưu file
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", file.FileName);

                    // Lưu file vào server
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    // Lưu tên file vào database
                    printJob.FileName = file.FileName;
                }

                // Lưu thông tin PrintJob vào database
                _context.Add(printJob);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            var username = HttpContext.Session.GetString("UserName");
            // Kiểm tra nếu username không null
            if (username != null)
            {
                // Tìm UserId từ bảng Users
                var userId = _context.Users
                                     .Where(u => u.UserName == username)
                                     .Select(u => u.UserId)
                                     .FirstOrDefault();

                // Truyền userId vào ViewBag
                ViewBag.UserId = userId;
            }

            ViewBag.PrinterId = new SelectList(_context.Printers, "PrinterId", "PrinterName");
            return View();
        }


        // GET: PrintJobs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var printJob = await _context.PrintJobs.FindAsync(id);
            if (printJob == null)
            {
                return NotFound();
            }
            ViewData["PrinterId"] = new SelectList(_context.Printers, "PrinterId", "PrinterId", printJob.PrinterId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", printJob.UserId);
            return View(printJob);
        }

        // POST: PrintJobs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PrintJobId,UserId,PrinterId,FileName,PageCount,TotalPages,Copies,IsDoubleSided,StartTime,EndTime")] PrintJob printJob)
        {
            if (id != printJob.PrintJobId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(printJob);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PrintJobExists(printJob.PrintJobId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["PrinterId"] = new SelectList(_context.Printers, "PrinterId", "PrinterId", printJob.PrinterId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", printJob.UserId);
            return View(printJob);
        }

        // GET: PrintJobs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var printJob = await _context.PrintJobs
                .Include(p => p.Printer)
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.PrintJobId == id);
            if (printJob == null)
            {
                return NotFound();
            }

            return View(printJob);
        }

        // POST: PrintJobs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var printJob = await _context.PrintJobs.FindAsync(id);
            if (printJob != null)
            {
                _context.PrintJobs.Remove(printJob);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        private bool PrintJobExists(int id)
        {
            return _context.PrintJobs.Any(e => e.PrintJobId == id);
        }
    }
}
