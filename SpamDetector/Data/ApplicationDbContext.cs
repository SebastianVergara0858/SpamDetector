using Microsoft.EntityFrameworkCore;
using SpamDetector.Models;

namespace SpamDetector.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<RequestLog> RequestLogs { get; set; }
    }
}