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
    public class UsersController : Controller
    {
        private readonly HcmsiuSspsContext _context;

        public UsersController(HcmsiuSspsContext context)
        {
            _context = context;
        }

        // GET: Admin/Users
        public async Task<IActionResult> Index()
        {
            return View(await _context.Users.ToListAsync());
        }

        // GET: Admin/Users/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Admin/Users/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserId,UserName,Email,Password,Role,PageBalance")] User user)
        {
            // Kiểm tra trùng username
            bool usernameExists = await _context.Users.AnyAsync(u => u.UserName == user.UserName);
            if (usernameExists)
            {
                TempData["Error"] = "Tên tài khoản đã tồn tại.";
                return RedirectToAction(nameof(Create));
            }

            // Kiểm tra trùng email
            bool emailExists = await _context.Users.AnyAsync(u => u.Email == user.Email);
            if (emailExists)
            {
                TempData["Error"] = "Email đã được sử dụng.";
                return RedirectToAction(nameof(Create));
            }
            var settingPage = await _context.SystemSettings
                                        .FirstOrDefaultAsync(s => s.SettingId == 1);
            var pageBal = 0;
            pageBal = int.Parse(settingPage.SettingValue);

            if (ModelState.IsValid)
            {
                _context.Add(user);
                user.PageBalance = pageBal;
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: Admin/Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
              .AsNoTracking()
             .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
            {
                return NotFound();
            }
            var uservm = new UserViewModel
            {
                UserId = user.UserId,
                FullName = user.FullName,
                UserName = user.UserName,
                Email = user.Email,
                Password = user.Password,
                Role = user.Role,
                PageBalance = user.PageBalance
            };
            return View(uservm);
        }

        // POST: Admin/Users/Edit/5


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UserId,FullName,UserName,Email,Password,Role,PageBalance")] UserViewModel user)
        {
            if (!ModelState.IsValid)
            {
                return View(user);
            }

            // Tìm đúng bản ghi đang được theo dõi (tracked) bởi DbContext
            var userToUpdate = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
            if (userToUpdate == null)
            {
                return NotFound();
            }

            // Nếu không nhập mật khẩu mới thì giữ nguyên mật khẩu cũ
            if (user.Password == null)
            {
                user.Password = userToUpdate.Password;
            }

            // Cập nhật thủ công các field
            userToUpdate.FullName = user.FullName;
            userToUpdate.UserName = user.UserName;
            userToUpdate.Email = user.Email;
            userToUpdate.Password = user.Password;
            userToUpdate.Role = user.Role;
            userToUpdate.PageBalance = user.PageBalance;

            try
            {
                await _context.SaveChangesAsync(); // Chỉ cần SaveChanges, không cần Update()
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }


        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, [Bind("UserId,FullName,UserName,Email,Password,Role,PageBalance")] UserViewModel user)
        //{
        //    if (user.Password == null)
        //    {
        //        var existingUser = _context.Users.FirstOrDefault(u => u.UserName == user.UserName);
        //        user.Password = existingUser.Password;
        //    }


        //    var user2 = new User
        //    {
        //        UserId = id,
        //        FullName = user.FullName,
        //        UserName = user.UserName,
        //        Email = user.Email,
        //        Password = user.Password,
        //        Role = user.Role,
        //        PageBalance = user.PageBalance
        //    };
        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            _context.Update(user2);
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!UserExists(user2.UserId))
        //            {
        //                return NotFound();
        //            }
        //            else
        //            {
        //                throw;
        //            }
        //        }
        //        return RedirectToAction(nameof(Index));
        //    }
        //    return View(user);
        //}

        // GET: Admin/Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Admin/Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.UserId == id);
        }
    }
}
