using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using myschool.Areas.Admin.Models;
using myschool.Models;
using myschool.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace myschool.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class LoginController : Controller
    {
        private readonly DataContext _context;
        public LoginController(DataContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Index(AdminUser user)
        {
            if (user == null) return NotFound();
            // Convert password to MD5
            string pw = Functions.MD5Password(user.Password ?? string.Empty);
            var check = _context.AdminUser
                .Select(u => new { u.UserID, u.UserName, u.Email, u.Password })
                .FirstOrDefault(u => u.UserName == user.UserName && u.Password == pw);
                
            if (check == null)
            {
                Functions._Message = "Login failed. Please check your username and password.";
                return RedirectToAction("Index", "Login");
            }
            Functions._Message = string.Empty;
            Functions._UserID = check.UserID;
            Functions._UserName = check.UserName ?? string.Empty;
            Functions._Email = check.Email ?? string.Empty;
            return RedirectToAction("Index", "Home");
        }
    }
}