﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HCMSIU_SSPS.Models;

namespace HCMSIU_SSPS.Controllers
{
    public class TransactionsController : Controller
    {
        private readonly HcmsiuSspsContext _context;

        public TransactionsController(HcmsiuSspsContext context)
        {
            _context = context;
        }

        // GET: Transactions
        public async Task<IActionResult> Index()
        {
            var username = HttpContext.Session.GetString("UserName");
            var filtered = _context.Transactions
                        .Include(p => p.User)
                        .Where(p => p.User.UserName == username);

            return View(await filtered.ToListAsync());
        }

        // GET: Transactions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transaction = await _context.Transactions
                .Include(t => t.User)
                .FirstOrDefaultAsync(m => m.TransactionId == id);
            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }

        // GET: Transactions/Create
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
            return View();
        }

        // POST: Transactions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TransactionId,UserId,Amount,Status,Description,Timestamp")] Transaction transaction)
        {
            transaction.Status = 0;
            transaction.Timestamp = DateTime.Now;
            transaction.TransactionId = GenerateUniquePrintJobId(); 
            if (ModelState.IsValid)
            {
                _context.Add(transaction);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", transaction.UserId);
            return View(transaction);
        }
        private int GenerateUniquePrintJobId()
        {
            Random random = new Random();
            int newId;

            do
            {
                // Tạo một số ngẫu nhiên
                newId = random.Next(100000, 999999); // Bạn có thể thay đổi phạm vi nếu muốn

            } while (_context.Transactions.Any(p => p.TransactionId == newId)); // Kiểm tra nếu đã có PrintJobId trong database

            return newId;
        }
        public async Task<IActionResult> GetRate()
        {
            var setting = await _context.SystemSettings
                                        .FirstOrDefaultAsync(s => s.SettingId == 4);

            if (setting != null)
            {
                return Json(new { rate = setting.SettingValue  }); // Trả về giá trị rate từ bảng SystemSettings
            }

            return Json(new { rate = 0 }); // Nếu không tìm thấy, trả về 0
        }

        // GET: Transactions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null)
            {
                return NotFound();
            }
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", transaction.UserId);
            return View(transaction);
        }

        // POST: Transactions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TransactionId,UserId,Amount,Status,Description,Timestamp")] Transaction transaction)
        {
            if (id != transaction.TransactionId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(transaction);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TransactionExists(transaction.TransactionId))
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
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", transaction.UserId);
            return View(transaction);
        }

        // GET: Transactions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transaction = await _context.Transactions
                .Include(t => t.User)
                .FirstOrDefaultAsync(m => m.TransactionId == id);
            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }

        // POST: Transactions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction != null)
            {
                _context.Transactions.Remove(transaction);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TransactionExists(int id)
        {
            return _context.Transactions.Any(e => e.TransactionId == id);
        }
    }
}
