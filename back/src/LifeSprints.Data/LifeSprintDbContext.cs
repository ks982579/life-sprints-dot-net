using Microsoft.EntityFrameworkCore;
using LifeSprints.Models;

namespace LifeSprints.Data
{
    // Microsoft::EntityFrameworkCore::DbContext;
    public class LifeSprintDbContext : DbContext
    {
        public LifeSprintDbContext(DbContextOptions<LifeSprintDbContext> options) : base(options)
        {

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
        }
    }
}


