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
        public DbSet<AdminUser> AdminUser { get; set; }
    }
}