using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LifeSprints.Models;

// https://learn.microsoft.com/en-us/ef/core/modeling/entity-properties?tabs=fluent-api%2Cwith-nrt
// There is explicit method calls OR data annotation ways. 

namespace LifeSprints.Data
{
    // Microsoft::EntityFrameworkCore::DbContext;
    public class LifeSprintDbContext : DbContext
    {
        public LifeSprintDbContext(DbContextOptions<LifeSprintDbContext> options) : base(options)
        {
            // Eat...
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Story> Stories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure table names to match PostgreSQL Schema
            modelBuilder.Entity<User>().ToTable("users");
            modelBuilder.Entity<Story>().ToTable("stories");

            // configure property mappings to match PostgreSQL naming
            // Private Functions first
            ConfigureUser(modelBuilder);
            ConfigureStory(modelBuilder);
            ConfigureRelationships(modelBuilder);
        }

        #region Private Methods
        private static void ConfigureUser(ModelBuilder modelBuilder)
        {
            // Microsoft.EntityFrameworkCore.Metadata.Builders::EntityTypeBuilder<T>
            EntityTypeBuilder<User> entity = modelBuilder.Entity<User>();
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.DisplayName).HasColumnName("display_name");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
        }

        private static void ConfigureStory(ModelBuilder modelBuilder)
        {
            // Microsoft.EntityFrameworkCore.Metadata.Builders::EntityTypeBuilder<T>
            EntityTypeBuilder<Story> entity = modelBuilder.Entity<Story>();
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Title).HasColumnName("title");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Year).HasColumnName("year");
            entity.Property(e => e.IsCompleted).HasColumnName("is_completed");
            entity.Property(e => e.Priority).HasColumnName("priority");
            entity.Property(e => e.EstimatedHours).HasColumnName("estimated_hours");
            entity.Property(e => e.ActualHours).HasColumnName("actual_hours");
            entity.Property(e => e.DueDate).HasColumnName("due_date");
            entity.Property(e => e.CompletedAt).HasColumnName("completed_at");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        }

        private static void ConfigureRelationships(ModelBuilder modelBuilder)
        {
            // Do I put this in Story?
            modelBuilder.Entity<Story>()
                .HasOne(m => m.User)
                .WithMany(a => a.Stories)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
        #endregion
    }
}


