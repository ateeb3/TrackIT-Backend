using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TrackIT.Core.Entities;

namespace TrackIT.Data.Context
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<AppUser>(options)
    {

        public DbSet<Asset> Assets => Set<Asset>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<AppUser> Employees => Set<AppUser>();
        public DbSet<AssignmentHistory> AssignmentHistories => Set<AssignmentHistory>();

        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Asset>().HasQueryFilter(a => !a.IsDeleted);
            modelBuilder.Entity<Category>().HasQueryFilter(c => !c.IsDeleted);
            modelBuilder.Entity<AppUser>().HasQueryFilter(u => !u.IsDeleted);

            // --- RELATIONSHIP CONFIGURATION ---

            // Ensure Asset Serial Number is Unique
            modelBuilder.Entity<Asset>()
                .HasIndex(a => a.SerialNumber)
                .IsUnique();

            
            modelBuilder.Entity<Category>()
                .HasMany(c => c.Assets)
                .WithOne(a => a.Category)
                .HasForeignKey(a => a.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AssignmentHistory>()
                .HasOne(h => h.User)
                .WithMany(u => u.AssignmentHistory)
                .HasForeignKey(h => h.UserId);
        }

        // 4. Intercepting Saves: This handles the Soft Delete logic automatically
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                // If we try to hard delete a BaseEntity...
                if (entry.State == EntityState.Deleted)
                {
                    // Stop! Don't delete it.
                    entry.State = EntityState.Modified;
                    // Just flip the flag.
                    entry.Entity.IsDeleted = true;
                    // Optional: Set UpdatedAt
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}