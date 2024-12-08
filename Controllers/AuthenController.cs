using HCMSIU_SSPS.Models;
using HCMSIU_SSPS.Services;
using Microsoft.AspNetCore.Mvc;

namespace HCMSIU_SSPS.Controllers
{
    public class AuthenController : Controller
    {
        private readonly HcmsiuSspsContext _context;
        private readonly IEmailService _emailService;

        public AuthenController(HcmsiuSspsContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }
        public IActionResult Index()
        {
            return View();
        }
        // POST: Authen/Login
        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            var user = _context.Users
                .FirstOrDefault(u => u.UserName == username && u.Password == password);

            if (user != null)
            {
                // Lưu thông tin người dùng vào Session
                HttpContext.Session.SetString("UserName", user.UserName);
                HttpContext.Session.SetString("Role", user.Role.ToString());

                // Thông báo đăng nhập thành công
                TempData["LoginSuccess"] = $"Đăng nhập thành công. Chào mừng, {user.UserName}!";
                return RedirectToAction("Index", "Home");
            }

            else
            {
                TempData["LoginError"] = "Tài khoản hoặc mật khẩu không đúng.";
                return RedirectToAction("Index", "Home"); // trả lại cái modal pop up login
            }
        }

        // POST: Authen/ForgotPassword
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string Email)
        {
            if (string.IsNullOrEmpty(Email))
            {
                TempData["ForgotPasswordError"] = "Email không được để trống.";
                return RedirectToAction("Index","Home"); // trả lại là cái modal pop up forgot pass
            }

            // Tìm người dùng trong cơ sở dữ liệu theo email
            var user = _context.Users.FirstOrDefault(u => u.Email == Email);
            if (user == null)
            {
                TempData["ForgotPasswordError"] = "Không tồn tại email.";
                return RedirectToAction("Index", "Home");
            }

            // Lấy mật khẩu hiện tại của người dùng
            string currentPassword = user.Password;  // Giả sử bạn lưu mật khẩu dưới dạng plain text

            // Gửi email với mật khẩu hiện tại
            string subject = "Thông tin mật khẩu của bạn";
            string body = $"Xin chào {user.UserName},<br><br>"
                        + "Mật khẩu hiện tại của bạn là: <strong>" + currentPassword + "</strong><br><br>"
                        + "Trân trọng,<br>HCMSIU SSPS";

            bool emailSent = await _emailService.SendEmailAsync(Email, subject, body);

            if (!emailSent)
            {
                // Xử lý khi gửi email thất bại
                TempData["EmailError"] = "Đã xảy ra lỗi khi gửi email.";
            }
            else
            {
                TempData["ForgotPasswordSuccess"] = $"Mật khẩu đã được gửi đến email {Email}.";
            }

            return RedirectToAction("Index", "Home" );
        }

    }
}
