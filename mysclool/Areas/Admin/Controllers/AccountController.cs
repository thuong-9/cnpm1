using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using mysclool.Areas.Admin.Models;
using mysclool.Models;

namespace mysclool.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AccountController : Controller
    {
        private readonly DataContext _context;
        public AccountController(DataContext context)
        {
            _context = context;
        }
        // Action Index - Hiển thị danh sách tài khoản
        public IActionResult Index()
        {
            // Lấy danh sách tài khoản từ bảng Account, sắp xếp theo UserID
            var mnList = _context.AdminUser.OrderBy(m => m.UserID).ToList();
            // Gửi danh sách sang View để hiển thị
            return View(mnList);
        }
        

        // Hiển thị danh sách tài khoản
        public IActionResult Delete(int? id)
        {
            var mnList = _context.AdminUser.OrderBy(m => m.UserID).ToList();

            AdminUser? deleteUser = null;
            if (id != null)
            {
                deleteUser = _context.AdminUser.Find(id);
            }

            ViewData["DeleteUser"] = deleteUser;
            return View(mnList);
        }

        // POST: thực hiện xóa
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int UserID)
        {
            var delUser = _context.AdminUser.Find(UserID);
            if (delUser != null)
            {
                _context.AdminUser.Remove(delUser);
                _context.SaveChanges();
            }

            return RedirectToAction("Delete");
        }

        public IActionResult Create()
        {
            var model = new AdminUser(); // khởi tạo model mới
            return View(model);
        }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(AdminUser model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Vui lòng điền đầy đủ thông tin hợp lệ!";
                return View(model);
            }

            try
            {
                _context.AdminUser.Add(model);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Tài khoản đã được tạo thành công!";
                return RedirectToAction("Create"); // trở lại trang tạo mới
            }
            catch
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi lưu dữ liệu!";
                return View(model);
            }
        }

        [HttpGet]
         public IActionResult Edit(int id)
        {
            var user = _context.AdminUser.FirstOrDefault(x => x.UserID == id);
            if (user != null)
            {
                ViewData["EditUser"] = user;
            }
            var list = _context.AdminUser.ToList();
            return View(list);
        }

        [HttpPost]
        public IActionResult EditConfirmed(AdminUser model)
        {
            var user = _context.AdminUser.FirstOrDefault(x => x.UserID == model.UserID);
            if (user != null)
            {
                user.UserName = model.UserName;
                user.Email = model.Email;
                user.IsActive = model.IsActive;
                _context.SaveChanges();
            }
            return RedirectToAction("Edit");
        }

    }
}