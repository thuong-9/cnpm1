using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
namespace myschool.Areas.Admin.Models
{
    [Table("Subjects")]
    public class tblSubjects
    {
        [Key]
        public int SubjectID { get; set; }
        public string? SubjectName { get; set; }
    }
}