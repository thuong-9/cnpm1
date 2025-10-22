using mysclool.Areas.Admin.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

namespace mysclool.Models
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }
        public DbSet<AdminMenu> AdminMenus { get; set; }
        public DbSet<AdminUser> AdminUser { get; set; }
        public DbSet<Class> Classes { get; set; } 
        public DbSet<Subjects> Subjects { get; set; }
    }
}