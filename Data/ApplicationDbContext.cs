using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using QuestPlanner.Models;

namespace QuestPlanner.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Trip> Trips { get; set; }
        public DbSet<Activity> Activities { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Конфигурация для Trip
            builder.Entity<Trip>(entity =>
            {
                entity.Property(t => t.StartDate)
                    .HasColumnType("timestamp without time zone");

                entity.Property(t => t.EndDate)
                    .HasColumnType("timestamp without time zone");

                entity.HasOne(t => t.User)
                    .WithMany(u => u.Trips)
                    .HasForeignKey(t => t.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Конфигурация для Activity
            builder.Entity<Activity>(entity =>
            {
                entity.Property(a => a.StartTime)
                    .HasColumnType("timestamp without time zone");

                entity.Property(a => a.EndTime)
                    .HasColumnType("timestamp without time zone");

                entity.HasOne(a => a.Trip)
                    .WithMany(t => t.Activities)
                    .HasForeignKey(a => a.TripId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}