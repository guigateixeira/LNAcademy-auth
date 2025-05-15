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
        public DbSet<Product> Products { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Module> Modules { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Users
            modelBuilder.Entity<User>().ToTable("users");
            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(u => u.Id)
                    .HasDefaultValueSql("gen_random_uuid()");
                entity.HasIndex(u => u.Email).IsUnique();
                
                entity.Property(u => u.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(u => u.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // Products
            modelBuilder.Entity<Product>().ToTable("products");
            modelBuilder.Entity<Product>(entity =>
            {
                entity.Property(p => p.Id)
                    .HasDefaultValueSql("gen_random_uuid()");
                
                entity.Property(p => p.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(p => p.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                
                entity.Property(p => p.Type)
                    .HasConversion<string>()
                    .HasMaxLength(50);
            
                entity.Property(p => p.Currency)
                    .HasConversion<string>()
                    .HasMaxLength(50);

                entity.Property(p => p.CoverImageUrl).IsRequired(false);
                
                // Configure inheritance
                entity.HasDiscriminator(p => p.Type)
                    .HasValue<Course>(ProductType.Course)
                    .HasValue<Book>(ProductType.Book);
                    
                // Foreign key to User (creator)
                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(p => p.CreatorId)
                    .OnDelete(DeleteBehavior.Restrict); // Don't delete products when a user is deleted
            });
            
            // Course configuration (inherits from Product)
            modelBuilder.Entity<Course>().ToTable("products");
            
            // Book configuration (inherits from Product)
            modelBuilder.Entity<Book>().ToTable("products");
            modelBuilder.Entity<Book>(entity =>
            {
                // Make PreviewUrl and DownloadUrl optional
                entity.Property(b => b.PreviewUrl).IsRequired(false);
                entity.Property(b => b.DownloadUrl).IsRequired(false);
            });
            
            // Module configuration
            modelBuilder.Entity<Module>().ToTable("modules");
            modelBuilder.Entity<Module>(entity =>
            {
                entity.Property(m => m.Id)
                    .HasDefaultValueSql("gen_random_uuid()");
                
                entity.Property(m => m.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(m => m.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                
                // Relationship with Course
                entity.HasOne(m => m.Course)
                    .WithMany(c => c.Modules)
                    .HasForeignKey(m => m.CourseId)
                    .OnDelete(DeleteBehavior.Cascade); // Delete modules when course is deleted
            });
            
            // Lesson configuration
            modelBuilder.Entity<Lesson>().ToTable("lessons");
            modelBuilder.Entity<Lesson>(entity =>
            {
                entity.Property(l => l.Id)
                    .HasDefaultValueSql("gen_random_uuid()");
                
                entity.Property(l => l.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(l => l.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                
                entity.Property(l => l.Type)
                    .HasConversion<string>()
                    .HasMaxLength(50);
                
                // Relationship with Module
                entity.HasOne(l => l.Module)
                    .WithMany(m => m.Lessons)
                    .HasForeignKey(l => l.ModuleId)
                    .OnDelete(DeleteBehavior.Cascade); // Delete lessons when module is deleted
            });
            
            // Add index on Order fields for sorting
            modelBuilder.Entity<Module>()
                .HasIndex(m => new { m.CourseId, m.Order });
                
            modelBuilder.Entity<Lesson>()
                .HasIndex(l => new { l.ModuleId, l.Order });
        }
    }
}