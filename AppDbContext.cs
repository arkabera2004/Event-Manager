using Microsoft.EntityFrameworkCore;
using CollegeEventManager.Models;

namespace CollegeEventManager.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<EventModel> Events => Set<EventModel>();
        public DbSet<RegistrationModel> Registrations => Set<RegistrationModel>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure the one-to-many relationship between Event and Registration with cascade delete
            modelBuilder.Entity<RegistrationModel>()
                .HasOne(r => r.Event)
                .WithMany(e => e.Registrations)
                .HasForeignKey(r => r.EventId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}