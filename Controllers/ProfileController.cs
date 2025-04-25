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

        // ở đây tôi muốn tạo 1 cái hàm  thay đổi mật khẩu, sẽ có kiểm tra 2 mật khẩu giống nhau hay không nói chung là oldpassword và newpassword, sẽ so sánh oldpassword với mật khẩu hiện tại, 2 mật khẩu mới, nếu 1 điều kiện sai thì return tempdata là lỗi khi bấm ok thì ở lại trang, nếu đúng thì return tempdata là đúng khi bấm ok ở tempdata sẽ đưa về trang chủ
        // GET: Hiển thị form đổi mật khẩu
        [HttpGet]
        public async Task<IActionResult> ChangePassword(string username)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == username);
            if (user == null) return NotFound();

            var model = new ChangePasswordViewModel
            {
                Username = user.UserName
            };

            return View(model); // View sẽ hiển thị form có sẵn username
        }

        // POST: Xử lý dữ liệu từ form
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == model.Username);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Người dùng không tồn tại.";
                return View(model);
            }

            if (user.Password != model.OldPassword)
            {
                TempData["ErrorMessage"] = "Mật khẩu cũ không đúng.";
                return View(model);
            }

            if (model.NewPassword != model.ConfirmPassword)
            {
                TempData["ErrorMessage"] = "Mật khẩu xác nhận không khớp.";
                return View(model);
            }

            user.Password = model.NewPassword;
            _context.Update(user);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
            return RedirectToAction("ChangePassword", new { username = model.Username });
        }


    }
}
