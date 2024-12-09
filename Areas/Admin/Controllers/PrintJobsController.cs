using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HCMSIU_SSPS.Models;

namespace HCMSIU_SSPS.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class PrintJobsController : Controller
    {
        private readonly HcmsiuSspsContext _context;

        public PrintJobsController(HcmsiuSspsContext context)
        {
            _context = context;
        }

        // GET: Admin/PrintJobs
        public async Task<IActionResult> Index()
        {
            var hcmsiuSspsContext = _context.PrintJobs.Include(p => p.Printer).Include(p => p.User);
            return View(await hcmsiuSspsContext.ToListAsync());
        }

        // GET: Admin/PrintJobs/Details/5
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


        // GET: Admin/PrintJobs/Edit/5
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

        // POST: Admin/PrintJobs/Edit/5
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

        // GET: Admin/PrintJobs/Delete/5
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

        // POST: Admin/PrintJobs/Delete/5
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
        [HttpPost]
        public async Task<IActionResult> ChangeStatus(int printJobId, int currentStatus)
        {
            // Tìm công việc in trong cơ sở dữ liệu
            var printJob = await _context.PrintJobs.FindAsync(printJobId);
            if (printJob == null)
            {
                return Json(new { success = false, message = "Print job not found." });
            }

            // Đổi trạng thái từ 0 thành 1 hoặc từ 1 thành 0
            var newStatus = currentStatus == 0 ? 1 : 0;
            printJob.Status = newStatus;

            // Cập nhật EndTime theo trạng thái
            DateTime? newEndTime = null;
            if (newStatus == 1)  // Nếu trạng thái mới là "Done", cập nhật EndTime
            {
                printJob.EndTime = DateTime.Now;
                newEndTime = printJob.EndTime;
            }
            else  // Nếu trạng thái mới là "Pending", set EndTime = null
            {
                printJob.EndTime = null;
            }

            // Lưu thay đổi vào database
            _context.PrintJobs.Update(printJob);
            await _context.SaveChangesAsync();

            // Trả về kết quả bao gồm trạng thái mới và EndTime (nếu có)
            return Json(new { success = true, newStatus = printJob.Status, newEndTime = newEndTime });
        }
        private bool PrintJobExists(int id)
        {
            return _context.PrintJobs.Any(e => e.PrintJobId == id);
        }
    }
}
