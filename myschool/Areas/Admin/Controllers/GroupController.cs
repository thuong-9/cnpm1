using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using myschool.Areas.Admin.Models;
using myschool.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace myschool.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class GroupController : Controller
    {
        private readonly DataContext _context;

        public GroupController(DataContext context)
        {
            _context = context;
        }

        // GET: Admin/Group
        public async Task<IActionResult> Index()
        {
            var groups = await _context.Groups.ToListAsync();
            return View(groups);
        }

        // GET: Admin/Group/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Group/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Group group)
        {
            if (ModelState.IsValid)
            {
                group.CreatedDate = DateTime.Now;

                // Try to set CreatedBy from the currently authenticated user
                int createdBy = 0;
                if (User?.Identity?.IsAuthenticated == true && !string.IsNullOrEmpty(User.Identity.Name))
                {
                    var currentUser = await _context.AdminUser.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
                    if (currentUser != null)
                    {
                        createdBy = currentUser.UserID;
                    }
                }

                // Fallback: use any existing admin user as creator to satisfy FK constraint
                if (createdBy == 0)
                {
                    var anyAdmin = await _context.AdminUser.FirstOrDefaultAsync();
                    if (anyAdmin != null)
                    {
                        createdBy = anyAdmin.UserID;
                    }
                    else
                    {
                        // No admin users exist -> cannot create group because FK requires a valid CreatedBy
                        ModelState.AddModelError(string.Empty, "Không có tài khoản quản trị nào trong hệ thống. Vui lòng tạo 1 Admin trước khi tạo nhóm.");
                        return View(group);
                    }
                }

                group.CreatedBy = createdBy;
                _context.Add(group);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(group);
        }

        // GET: Admin/Group/Members
        public async Task<IActionResult> Members(int? id)
        {
            var query = _context.GroupMembers
                .Include(gm => gm.Group)
                .Include(gm => gm.User)
                .AsQueryable();

            if (id.HasValue)
            {
                query = query.Where(gm => gm.GroupId == id.Value);

                // Load group info for header and route links
                var group = await _context.Groups.FindAsync(id.Value);
                ViewBag.Group = group ?? new Group { GroupName = "(Không rõ)" };
                ViewBag.GroupId = id.Value;
            }
            else
            {
                // No specific group: show generic header
                ViewBag.Group = new Group { GroupName = "Tất cả nhóm" };
                ViewBag.GroupId = null;
            }

            var members = await query.ToListAsync();
            return View(members);
        }

        // GET: Admin/Group/AddMember
        public async Task<IActionResult> AddMember()
        {
            ViewBag.Users = await _context.AdminUser
                .Select(u => new { u.UserID, u.UserName })
                .ToListAsync();
            ViewBag.Groups = await _context.Groups
                .Select(g => new { g.GroupId, g.GroupName })
                .ToListAsync();

            return View(new GroupMember());
        }

        // POST: Admin/Group/AddMember
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMember(GroupMember groupMember)
        {
            if (ModelState.IsValid)
            {
                groupMember.JoinedDate = DateTime.Now;
                _context.Add(groupMember);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Members), new { id = groupMember.GroupId });
            }
            ViewBag.Users = await _context.AdminUser
                .Select(u => new { u.UserID, u.UserName })
                .ToListAsync();
            ViewBag.Groups = await _context.Groups
                .Select(g => new { g.GroupId, g.GroupName })
                .ToListAsync();
            return View(groupMember);
        }

        // GET: Admin/Group/EditMember
        public async Task<IActionResult> EditMember(int? id = null)
        {
            // Load all users and groups for dropdowns regardless of context
            ViewBag.Users = await _context.AdminUser
                .Select(u => new { u.UserID, u.UserName })
                .ToListAsync();
            ViewBag.Groups = await _context.Groups
                .Select(g => new { g.GroupId, g.GroupName })
                .ToListAsync();

            // If no ID provided, return an empty form for new member
            if (id == null)
            {
                return View(new GroupMember());
            }

            // Try to find the member if ID is provided
            var groupMember = await _context.GroupMembers
                .Include(gm => gm.Group)
                .Include(gm => gm.User)
                .FirstOrDefaultAsync(m => m.GroupMemberId == id.Value);

            if (groupMember == null)
            {
                return NotFound();
            }

            return View(groupMember);
        }

        // GET: Admin/Group/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var group = await _context.Groups.FindAsync(id);
            if (group == null)
            {
                return NotFound();
            }

            return View(group);
        }

        // POST: Admin/Group/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Group group)
        {
            if (id != group.GroupId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _context.Groups.FindAsync(id);
                    if (existing != null)
                    {
                        existing.GroupName = group.GroupName;
                        existing.Description = group.Description;
                        await _context.SaveChangesAsync();
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(group);
        }

        // POST: Admin/Group/EditMember/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("EditMember")]
        public async Task<IActionResult> EditMemberPost(int? id, GroupMember groupMember)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (id.HasValue)
                    {
                        // Updating existing member
                        var existingMember = await _context.GroupMembers.FindAsync(id.Value);
                        if (existingMember != null)
                        {
                            existingMember.Role = groupMember.Role;
                            existingMember.UserId = groupMember.UserId;
                            existingMember.GroupId = groupMember.GroupId;
                            await _context.SaveChangesAsync();
                        }
                    }
                    else
                    {
                        // Adding new member
                        groupMember.JoinedDate = DateTime.Now;
                        _context.Add(groupMember);
                        await _context.SaveChangesAsync();
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GroupMemberExists(groupMember.GroupMemberId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Members), new { id = groupMember.GroupId });
            }
            return View(groupMember);
        }

        // POST: Admin/Group/DeleteMember/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMember(int id)
        {
            var member = await _context.GroupMembers.FindAsync(id);
            if (member == null)
            {
                return NotFound();
            }

            var groupId = member.GroupId;
            _context.GroupMembers.Remove(member);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Members), new { id = groupId });
        }

        // POST: Admin/Group/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var group = await _context.Groups.FindAsync(id);
            if (group != null)
            {
                // remove members first
                var members = _context.GroupMembers.Where(gm => gm.GroupId == id);
                _context.GroupMembers.RemoveRange(members);

                _context.Groups.Remove(group);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool GroupMemberExists(int id)
        {
            return _context.GroupMembers.Any(e => e.GroupMemberId == id);
        }
    }
}