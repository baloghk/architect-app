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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<QuoteRequest>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.PublicId).IsUnique();
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PhoneNumber).IsRequired();
                entity.Property(e => e.ProjectType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Location).IsRequired().HasMaxLength(100);
                entity.Property(e => e.HousePlanPath).IsRequired();
                entity.Property(e => e.StartOfProject).IsRequired();
                entity.Property(e => e.EndOfProject).IsRequired();
                entity.Property(e => e.Description).IsRequired().HasMaxLength(1000);

                entity.Property(e => e.PublicId)
                    .HasDefaultValueSql("NEWID()");
            });
        }
    }
}
