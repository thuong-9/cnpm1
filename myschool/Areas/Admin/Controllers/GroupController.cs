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

        // GET: Admin/Group/Delete (show inline modal on Index)
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            var list = await _context.Groups.ToListAsync();

            Group? deleteGroup = null;
            if (id.HasValue)
            {
                deleteGroup = await _context.Groups.FindAsync(id.Value);
            }

            ViewData["DeleteGroup"] = deleteGroup;
            // Reuse Index view, which will show the modal when DeleteGroup is set
            return View("Index", list);
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
                    var currentUser = await _context.AdminUsers.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
                    if (currentUser != null)
                    {
                        createdBy = currentUser.UserID;
                    }
                }

                // Fallback: use any existing admin user as creator to satisfy FK constraint
                if (createdBy == 0)
                {
                    var anyAdmin = await _context.AdminUsers.FirstOrDefaultAsync();
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

        // GET: Admin/Group/DeleteMember (show inline modal on Members)
        [HttpGet]
        [ActionName("DeleteMember")]
        public async Task<IActionResult> ConfirmDeleteMember(int id)
        {
            var member = await _context.GroupMembers
                .Include(gm => gm.Group)
                .Include(gm => gm.User)
                .FirstOrDefaultAsync(gm => gm.GroupMemberId == id);
            if (member == null)
            {
                return NotFound();
            }

            var gid = member.GroupId;
            var members = await _context.GroupMembers
                .Include(gm => gm.Group)
                .Include(gm => gm.User)
                .Where(gm => gm.GroupId == gid)
                .OrderBy(gm => gm.User != null ? gm.User.UserName : string.Empty)
                .ToListAsync();

            ViewBag.Group = await _context.Groups.FindAsync(gid) ?? new Group { GroupName = "(Không rõ)" };
            ViewBag.GroupId = gid;
            ViewData["DeleteMember"] = member;

            // Reuse Members view, which will show the modal when DeleteMember is set
            return View("Members", members);
        }

        // GET: Admin/Group/AddMember
        public async Task<IActionResult> AddMember(int? id, string? groupName)
        {
            // Dropdown sources
            ViewBag.Users = await _context.AdminUsers
                .Select(u => new { u.UserID, u.UserName })
                .ToListAsync();
            ViewBag.Groups = await _context.Groups
                .Select(g => new { g.GroupId, g.GroupName })
                .ToListAsync();

            // Resolve group by name if provided
            if (!id.HasValue && !string.IsNullOrWhiteSpace(groupName))
            {
                var key = groupName.Trim().ToLowerInvariant();
                var g = await _context.Groups
                    .Where(x => x.GroupName != null && x.GroupName.ToLower() == key)
                    .Select(x => new { x.GroupId, x.GroupName })
                    .FirstOrDefaultAsync();
                if (g != null)
                {
                    id = g.GroupId;
                    ViewBag.SelectedGroupName = g.GroupName;
                }
                else
                {
                    TempData["ErrorMessage"] = "Không tìm thấy nhóm theo tên đã nhập.";
                }
            }

            // If a group is selected, load its members to display on the same page
            if (id.HasValue)
            {
                var members = await _context.GroupMembers
                    .Include(gm => gm.Group)
                    .Include(gm => gm.User)
                    .Where(gm => gm.GroupId == id.Value)
                    .OrderBy(gm => gm.User != null ? gm.User.UserName : string.Empty)
                    .ToListAsync();

                ViewBag.Members = members;
                ViewBag.SelectedGroupId = id.Value;
                if (ViewBag.SelectedGroupName == null)
                {
                    var gname = await _context.Groups.Where(g => g.GroupId == id.Value).Select(g => g.GroupName).FirstOrDefaultAsync();
                    ViewBag.SelectedGroupName = gname;
                }
            }

            // Preselect the chosen group in the form
            return View(new GroupMember { GroupId = id ?? 0 });
        }

        // POST: Admin/Group/AddMember
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMember(GroupMember groupMember)
        {
            if (ModelState.IsValid)
            {
                // Prevent duplicate membership
                var exists = await _context.GroupMembers
                    .AnyAsync(gm => gm.GroupId == groupMember.GroupId && gm.UserId == groupMember.UserId);
                if (exists)
                {
                    TempData["ErrorMessage"] = "Thành viên đã tồn tại trong nhóm này.";
                    return RedirectToAction(nameof(AddMember), new { id = groupMember.GroupId });
                }

                groupMember.JoinedDate = DateTime.Now;
                _context.Add(groupMember);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã thêm thành viên vào nhóm thành công.";
                // Stay on AddMember page and keep the selected group context
                return RedirectToAction(nameof(AddMember), new { id = groupMember.GroupId });
            }
            ViewBag.Users = await _context.AdminUsers
                .Select(u => new { u.UserID, u.UserName })
                .ToListAsync();
            ViewBag.Groups = await _context.Groups
                .Select(g => new { g.GroupId, g.GroupName })
                .ToListAsync();
            // When validation fails, also reload current members for the selected group
            if (groupMember.GroupId > 0)
            {
                var members = await _context.GroupMembers
                    .Include(gm => gm.Group)
                    .Include(gm => gm.User)
                    .Where(gm => gm.GroupId == groupMember.GroupId)
                    .OrderBy(gm => gm.User != null ? gm.User.UserName : string.Empty)
                    .ToListAsync();
                ViewBag.Members = members;
                ViewBag.SelectedGroupId = groupMember.GroupId;
            }

            return View(groupMember);
        }

        // GET: Admin/Group/EditMember
        public async Task<IActionResult> EditMember(int? id = null, int? groupId = null, string? groupName = null)
        {
            // Load all users and groups for dropdowns regardless of context
            ViewBag.Users = await _context.AdminUsers
                .Select(u => new { u.UserID, u.UserName })
                .ToListAsync();
            ViewBag.Groups = await _context.Groups
                .Select(g => new { g.GroupId, g.GroupName })
                .ToListAsync();

            // Resolve group by name if provided and groupId not given
            if (!groupId.HasValue && !string.IsNullOrWhiteSpace(groupName))
            {
                var key = groupName.Trim().ToLowerInvariant();
                var g = await _context.Groups
                    .Where(x => x.GroupName != null && x.GroupName.ToLower() == key)
                    .Select(x => new { x.GroupId, x.GroupName })
                    .FirstOrDefaultAsync();
                if (g != null)
                {
                    groupId = g.GroupId;
                    ViewBag.SelectedGroupName = g.GroupName;
                }
                else
                {
                    TempData["ErrorMessage"] = "Không tìm thấy nhóm theo tên đã nhập.";
                }
            }

            // If editing a specific member (id provided)
            if (id.HasValue)
            {
                var groupMember = await _context.GroupMembers
                    .Include(gm => gm.Group)
                    .Include(gm => gm.User)
                    .FirstOrDefaultAsync(m => m.GroupMemberId == id.Value);

                if (groupMember == null)
                {
                    return NotFound();
                }

                // Also load list for the member's group to show context
                var gid = groupMember.GroupId;
                var members = await _context.GroupMembers
                    .Include(gm => gm.Group)
                    .Include(gm => gm.User)
                    .Where(gm => gm.GroupId == gid)
                    .OrderBy(gm => gm.User != null ? gm.User.UserName : string.Empty)
                    .ToListAsync();
                ViewBag.Members = members;
                ViewBag.SelectedGroupId = gid;
                ViewBag.SelectedGroupName = await _context.Groups.Where(g => g.GroupId == gid).Select(g => g.GroupName).FirstOrDefaultAsync();

                return View(groupMember);
            }

            // If group is selected by id (or by name above), show an empty form bound to that group and list members
            if (groupId.HasValue)
            {
                var members = await _context.GroupMembers
                    .Include(gm => gm.Group)
                    .Include(gm => gm.User)
                    .Where(gm => gm.GroupId == groupId.Value)
                    .OrderBy(gm => gm.User != null ? gm.User.UserName : string.Empty)
                    .ToListAsync();
                ViewBag.Members = members;
                ViewBag.SelectedGroupId = groupId.Value;
                if (ViewBag.SelectedGroupName == null)
                {
                    ViewBag.SelectedGroupName = await _context.Groups.Where(g => g.GroupId == groupId.Value).Select(g => g.GroupName).FirstOrDefaultAsync();
                }
                return View(new GroupMember { GroupId = groupId.Value });
            }

            // No selection -> empty page with hint
            return View(new GroupMember());
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
                        // Adding new member (avoid duplicate)
                        var exists = await _context.GroupMembers
                            .AnyAsync(gm => gm.GroupId == groupMember.GroupId && gm.UserId == groupMember.UserId);
                        if (exists)
                        {
                            TempData["ErrorMessage"] = "Thành viên đã tồn tại trong nhóm này.";
                            return RedirectToAction(nameof(AddMember), new { id = groupMember.GroupId });
                        }
                        groupMember.JoinedDate = DateTime.Now;
                        _context.Add(groupMember);
                        await _context.SaveChangesAsync();
                        TempData["SuccessMessage"] = "Đã thêm thành viên vào nhóm thành công.";
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