using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using myschool.Areas.Admin.Models;
using myschool.Models;

namespace myschool.Areas.Admin.Controllers
{
    [Area ("Admin")]
    public class ClassController : Controller
    {
        private readonly DataContext _context;
        public ClassController(DataContext context)
        {
            _context = context;
        }
        // Action Index - Hiển thị danh sách tài khoản
        public IActionResult Index()
        {
            // Lấy danh sách tài khoản từ bảng Subject, sắp xếp theo ClassID
            var mnList = _context.Classes.OrderBy(m => m.ClassID).ToList();
            // Gửi danh sách sang View để hiển thị
            return View(mnList);
        }
        public IActionResult Create()
        {
            var model = new Class(); // khởi tạo model mới
            return View(model);
        }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Class model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Vui lòng điền lớp học hợp lệ!";
                return View(model);
            }

            try
            {
                _context.Classes.Add(model);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Lớp học đã được tạo thành công!";
                return RedirectToAction("Index"); // trở lại trang danh sách
            }
            catch
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi lưu dữ liệu!";
                return View(model);
            }
        }

         // Hiển thị danh sách tài khoản
        public IActionResult Delete(int? id)
        {
            var mnList = _context.Classes.OrderBy(m => m.ClassID).ToList();

            Class? deleteName = null;
            if (id != null)
            {
                deleteName = _context.Classes.Find(id);
            }

            ViewData["DeleteName"] = deleteName;
            return View(mnList);
        }

        // POST: thực hiện xóa
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int ClassID)
        {
            var delName = _context.Classes.Find(ClassID);
            if (delName != null)
            {
                _context.Classes.Remove(delName);
                _context.SaveChanges();
            }

            return RedirectToAction("Delete");
        }
        [HttpGet]
         public IActionResult Edit(int id)
        {
            var name = _context.Classes.FirstOrDefault(x => x.ClassID == id);
            if (name != null)
            {
                ViewData["EditName"] = name;
            }
            var list = _context.Classes.ToList();
            return View(list);
        }

        [HttpPost]
        public IActionResult EditConfirmed(Class model)
        {
            var name = _context.Classes.FirstOrDefault(x => x.ClassID == model.ClassID);
            if (name != null)
            {
                name.ClassID = model.ClassID;
                name.ClassName = model.ClassName;
                _context.SaveChanges();
            }
            return RedirectToAction("Edit");
        }

    }
}