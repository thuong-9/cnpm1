using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace mysclool.Areas.Admin.Models
{
    [Table("Class")]
    public class Class
    {
        [Key]
        public int ClassID { get; set; }
        public string? ClassName { get; set; }
    }
}