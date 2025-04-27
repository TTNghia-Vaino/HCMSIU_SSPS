using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HCMSIU_SSPS.Models;
using System.Reflection.PortableExecutable;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;


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
        public async Task<IActionResult> Index()
        {
            var username = HttpContext.Session.GetString("UserName");
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
        public async Task<IActionResult> Create([Bind("PrintJobId,UserId,PrinterId,PageCount,TotalPages,Copies,IsA3,IsDoubleSided,StartTime,EndTime")] PrintJob printJob, IFormFile file)
        {
            // Tạo ID cho PrintJob mới và gán các giá trị mặc định
            printJob.PrintJobId = GenerateUniquePrintJobId();
            printJob.Status = 0; // Trạng thái là chưa hoàn thành
            printJob.StartTime = DateTime.Now;
            printJob.EndTime = null; // Chưa có thời gian kết thúc

            // Kiểm tra và xử lý file tải lên
            if (file != null && file.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var filePath = Path.Combine(uploadsFolder, file.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream); // Lưu file vào thư mục
                }

                printJob.FileName = file.FileName;

                // Lấy số trang từ file tùy thuộc vào định dạng file
                var fileExtension = Path.GetExtension(file.FileName).ToLower();
                switch (fileExtension)
                {
                    case ".pdf":
                        printJob.PageCount = GetPdfPageCount(filePath);
                        break;

                    case ".pptx":
                        printJob.PageCount = GetPptxSlideCount(filePath);
                        break;

                    default:
                        ModelState.AddModelError("File", "Only PDF and PPTX files are supported.");
                        break;
                }
            }

            // Nếu Model không hợp lệ, trả về lỗi
            if (!ModelState.IsValid)
            {
                var username = HttpContext.Session.GetString("UserName");
                if (username != null)
                {
                    var userId = _context.Users
                                         .Where(u => u.UserName == username)
                                         .Select(u => u.UserId)
                                         .FirstOrDefault();
                    ViewBag.UserId = userId;
                }

                ViewBag.PrinterId = new SelectList(_context.Printers, "PrinterId", "PrinterName");
                return View(printJob);
            }

            // Lấy thông tin người dùng từ session
            var user = await _context.Users
                                     .FirstOrDefaultAsync(u => u.UserName == HttpContext.Session.GetString("UserName"));

            // Trừ PageBalance của người dùng
            user.PageBalance -= printJob.TotalPages;

            // Cập nhật thông tin người dùng vào cơ sở dữ liệu
            _context.Update(user);
            await _context.SaveChangesAsync();

            // Lưu PrintJob vào database
            _context.Add(printJob);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public JsonResult CheckPageBalance(int totalPages)
        {
            var username = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrEmpty(username))
            {
                return Json(new { isPageBalanceSufficient = false });
            }

            // Tìm thông tin người dùng từ bảng Users
            var user = _context.Users.FirstOrDefault(u => u.UserName == username);
            if (user != null)
            {
                bool isSufficient = user.PageBalance >= totalPages;
                return Json(new { isPageBalanceSufficient = isSufficient });
            }

            return Json(new { isPageBalanceSufficient = false });
        }
        private int GenerateUniquePrintJobId()
        {
            Random random = new Random();
            int newId;

            do
            {
                // Tạo một số ngẫu nhiên
                newId = random.Next(100000, 999999); // Bạn có thể thay đổi phạm vi nếu muốn

            } while (_context.PrintJobs.Any(p => p.PrintJobId == newId)); // Kiểm tra nếu đã có PrintJobId trong database

            return newId;
        }
        [HttpPost]
        public JsonResult GetPageCount(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return Json(new { success = false, message = "No file uploaded." });
            }

            try
            {
                // Lưu file tạm thời để xử lý
                var tempFilePath = Path.GetTempFileName();
                using (var stream = new FileStream(tempFilePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                int pageCount = 0;

                // Kiểm tra loại file
                var extension = Path.GetExtension(file.FileName).ToLower();
                if (extension == ".pdf")
                {
                    pageCount = GetPdfPageCount(tempFilePath);
                }
                else if (extension == ".pptx")
                {
                    pageCount = GetPptxSlideCount(tempFilePath);
                }
                else
                {
                    return Json(new { success = false, message = "Xin lỗi, hệ thống không hỗ trợ loại file bạn gửi lên. Vui lòng liên hệ quản lý để biết thêm!" });
                }

                // Xóa file tạm sau khi xử lý
                if (System.IO.File.Exists(tempFilePath))
                {
                    System.IO.File.Delete(tempFilePath);
                }

                return Json(new { success = true, pageCount });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private int GetPdfPageCount(string pdfFilePath)
        {
            // Logic để tính số trang PDF (có thể dùng thư viện PdfSharp hoặc iTextSharp)
            using (var document = PdfReader.Open(pdfFilePath, PdfDocumentOpenMode.ReadOnly))
            {
                return document.PageCount;
            }
        }

        public int GetPptxSlideCount(string filePath)
        {
            using (PresentationDocument presentation = PresentationDocument.Open(filePath, false))
            {
                return presentation.PresentationPart.SlideParts.Count();
            }
        }

        [HttpPost]
        public IActionResult CalculateTotalPages(int pageCount, bool isA3, bool isDoubleSided, int copies)
        {
            // Tính TotalPages bằng logic đã đề xuất
            int actualPages = isA3 ? pageCount * 2 : pageCount;
            int pagesToPrint = isDoubleSided ? (int)Math.Ceiling(actualPages / 2.0) : actualPages;
            int totalPages = pagesToPrint * copies;

            return Json(new { totalPages = totalPages });
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
