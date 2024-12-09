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
    public class PrintersController : Controller
    {
        private readonly HcmsiuSspsContext _context;

        public PrintersController(HcmsiuSspsContext context)
        {
            _context = context;
        }

        // GET: Admin/Printers
        public async Task<IActionResult> Index()
        {
            return View(await _context.Printers.ToListAsync());
        }

        // GET: Admin/Printers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var printer = await _context.Printers
                .FirstOrDefaultAsync(m => m.PrinterId == id);
            if (printer == null)
            {
                return NotFound();
            }

            return View(printer);
        }

        // GET: Admin/Printers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Printers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PrinterId,PrinterName,Brand,Model,Location,IsEnable")] Printer printer)
        {
            printer.IsEnable = 1; // Enable after Create a new printer
            if (ModelState.IsValid)
            {
                _context.Add(printer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(printer);
        }

        // GET: Admin/Printers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var printer = await _context.Printers.FindAsync(id);
            if (printer == null)
            {
                return NotFound();
            }
            return View(printer);
        }

        // POST: Admin/Printers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PrinterId,PrinterName,Brand,Model,Location,IsEnable")] Printer printer)
        {
            if (id != printer.PrinterId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(printer);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PrinterExists(printer.PrinterId))
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
            return View(printer);
        }

        // GET: Admin/Printers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var printer = await _context.Printers
                .FirstOrDefaultAsync(m => m.PrinterId == id);
            if (printer == null)
            {
                return NotFound();
            }

            return View(printer);
        }

        // POST: Admin/Printers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var printer = await _context.Printers.FindAsync(id);
            if (printer != null)
            {
                _context.Printers.Remove(printer);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PrinterExists(int id)
        {
            return _context.Printers.Any(e => e.PrinterId == id);
        }
    }
}
