using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using mysclool.Areas.Admin.Models;
using mysclool.Models;

namespace mysclool.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SubjectController : Controller
    {
        private readonly DataContext _context;
        public SubjectController(DataContext context)
        {
            _context = context;
        }
        // Action Index - Hiển thị danh sách tài khoản
        public IActionResult Index()
        {
            // Lấy danh sách tài khoản từ bảng Subject, sắp xếp theo SubjectID
            var mnList = _context.Subjects.OrderBy(m => m.SubjectID).ToList();
            // Gửi danh sách sang View để hiển thị
            return View(mnList);
        }
        public IActionResult Create()
        {
            var model = new Subjects(); // khởi tạo model mới
            return View(model);
        }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Subjects model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Vui lòng điền môn học hợp lệ!";
                return View(model);
            }

            try
            {
                _context.Subjects.Add(model);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Môn học đã được tạo thành công!";
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
            var mnList = _context.Subjects.OrderBy(m => m.SubjectID).ToList();

            Subjects? deleteName = null;
            if (id != null)
            {
                deleteName = _context.Subjects.Find(id);
            }

            ViewData["DeleteName"] = deleteName;
            return View(mnList);
        }

        // POST: thực hiện xóa
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int SubjectID)
        {
            var delName = _context.Subjects.Find(SubjectID);
            if (delName != null)
            {
                _context.Subjects.Remove(delName);
                _context.SaveChanges();
            }

            return RedirectToAction("Delete");
        }
        [HttpGet]
         public IActionResult Edit(int id)
        {
            var name = _context.Subjects.FirstOrDefault(x => x.SubjectID == id);
            if (name != null)
            {
                ViewData["EditName"] = name;
            }
            var list = _context.Subjects.ToList();
            return View(list);
        }

        [HttpPost]
        public IActionResult EditConfirmed(Subjects model)
        {
            var name = _context.Subjects.FirstOrDefault(x => x.SubjectID == model.SubjectID);
            if (name != null)
            {
                name.SubjectID = model.SubjectID;
                name.SubjectName = model.SubjectName;
                _context.SaveChanges();
            }
            return RedirectToAction("Edit");
        }


        
    }
}