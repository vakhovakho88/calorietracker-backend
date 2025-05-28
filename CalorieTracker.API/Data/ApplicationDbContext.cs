using CalorieTracker.API.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace CalorieTracker.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Goal> Goals { get; set; }
        public DbSet<DailyLog> DailyLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Goal entity
            modelBuilder.Entity<Goal>()
                .HasIndex(g => g.UserId)
                .HasDatabaseName("IX_Goals_UserId");

            // Configure DailyLog entity
            modelBuilder.Entity<DailyLog>()
                .HasIndex(d => new { d.UserId, d.Date })
                .IsUnique()
                .HasDatabaseName("IX_DailyLogs_UserId_Date");

            // Add any additional configurations here
        }
    }
}