using NetTechnology_Final.Models;
using Microsoft.EntityFrameworkCore;
namespace NetTechnology_Final.Context
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Accounts> Accounts { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Products> Products { get; set; }
    }
}
