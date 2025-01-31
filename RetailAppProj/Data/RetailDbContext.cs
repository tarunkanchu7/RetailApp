
using Microsoft.EntityFrameworkCore;
using RetailApp.Models;

namespace RetailApp.Data
{
    public class RetailDbContext : DbContext
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<ApprovalQueue> ApprovalQueue { get; set; }

        public RetailDbContext(DbContextOptions<RetailDbContext> options) : base(options) { }
    }
}
