using ArchitectApp.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ArchitectApp.Data
{
    public class ArchitectDbContext : IdentityDbContext
    {
        public ArchitectDbContext(DbContextOptions<ArchitectDbContext> options)
            : base(options)
        {
        }

        public DbSet<QuoteRequest> QuoteRequests { get; set; }
    }
}
