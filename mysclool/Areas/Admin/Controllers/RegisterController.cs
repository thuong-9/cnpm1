using mysclool.Areas.Admin.Models;
using mysclool.Models;
using mysclool.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace mysclool.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class RegisterController : Controller
    {
    private readonly DataContext _context;
    private readonly ILogger<RegisterController> _logger;

        public RegisterController(DataContext context, ILogger<RegisterController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(AdminUser auser)
        {
            _logger.LogInformation("Register POST called");
            if (auser == null)
            {
                _logger.LogWarning("auser is null");
                return NotFound();
            }

            _logger.LogInformation("ModelState.IsValid = {IsValid}", ModelState.IsValid);
            try
            {
                _logger.LogInformation("Request.HasFormContentType = {HasForm}", Request.HasFormContentType);
                if (Request.HasFormContentType)
                {
                    foreach (var key in Request.Form.Keys)
                    {
                        _logger.LogInformation("Form[{Key}] = {Value}", key, Request.Form[key]);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading Request.Form");
            }

            _logger.LogInformation("Received user: UserName={UserName}, Email={Email}, IsActive={IsActive}", auser.UserName, auser.Email, auser.IsActive);

            // Validate model
            if (!ModelState.IsValid)
            {
                Functions._Message = "Invalid input";
                return View(auser);
            }

            // Guard: avoid querying when username is null/empty
            if (string.IsNullOrWhiteSpace(auser.UserName))
            {
                Functions._Message = "Username is required";
                return View(auser);
            }

            // Check the duplicate username before registering
            var check = _context.AdminUser.Where(u => u.UserName == auser.UserName).FirstOrDefault();
            if (check != null) //Already exists this username
            {
                Functions._Message = "Username already exists";
                return RedirectToAction("Index", "Register");
            }

            //If username does not exist
            Functions._Message = string.Empty;
            auser.Password = Functions.MD5Password(auser.Password);
            _context.AdminUser.Add(auser);
            await _context.SaveChangesAsync();
            _logger.LogInformation("User saved: {UserName} (id={Id})", auser.UserName, auser.UserID);
            return RedirectToAction("Index", "Login");
        }
    }
}