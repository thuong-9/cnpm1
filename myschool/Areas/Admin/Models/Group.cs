using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace myschool.Areas.Admin.Models
{
    [Table("Groups")]
    public class Group
    {
        [Key]
        public int GroupId { get; set; }

        [Required]
        [StringLength(100)]
        public string? GroupName { get; set; }

        [StringLength(255)]
        public string? Description { get; set; }

        public DateTime CreatedDate { get; set; }

        public int CreatedBy { get; set; }

        // Navigation property
        public virtual ICollection<GroupMember>? GroupMembers { get; set; }
    }
}