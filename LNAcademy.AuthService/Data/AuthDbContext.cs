// Data/AuthDbContext.cs

using LNAcademy.AuthService.Models;
using Microsoft.EntityFrameworkCore;

namespace LNAcademy.AuthService.Data
{
    public class AuthDbContext : DbContext
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options)
            : base(options)
        {
        }
        
        public DbSet<User> Users { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().ToTable("users");
            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(u => u.Id)
                    .HasDefaultValueSql("gen_random_uuid()");
                entity.HasIndex(u => u.Email).IsUnique();
                
                entity.Property(u => u.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(u => u.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

        }
    }
}