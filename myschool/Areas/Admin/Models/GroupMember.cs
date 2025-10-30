using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace myschool.Areas.Admin.Models
{
    [Table("GroupMembers")]
    public class GroupMember
    {
        [Key]
        public int GroupMemberId { get; set; }

        public int GroupId { get; set; }

        public int UserId { get; set; }

        [StringLength(50)]
        public string? Role { get; set; }

        public DateTime JoinedDate { get; set; }

        // Navigation property
        public virtual Group? Group { get; set; }
        public virtual tblAdminUser? User { get; set; }
    }
}