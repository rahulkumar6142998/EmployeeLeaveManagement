using EmployeeLeaveManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLeaveManagement.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<LeaveRequest> LeaveRequests { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Role).HasDefaultValue("Employee");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
            });

            // Configure LeaveRequest entity
            modelBuilder.Entity<LeaveRequest>(entity =>
            {
                entity.HasOne(lr => lr.User)
                    .WithMany(u => u.LeaveRequests)
                    .HasForeignKey(lr => lr.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(lr => lr.ProcessedByAdmin)
                    .WithMany()
                    .HasForeignKey(lr => lr.ProcessedBy)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(false);

                entity.Property(e => e.Status).HasDefaultValue("Pending");
            });

            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasOne(a => a.LeaveRequest)
                    .WithMany()
                    .HasForeignKey(a => a.LeaveRequestId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.User)
                    .WithMany()
                    .HasForeignKey(a => a.ActionBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Seed default Admin and Employee users
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    UserId = 1,
                    FullName = "System Admin",
                    Email = "admin@example.com",
                    Password = "admin123", 
                    Role = "Admin",
                    IsActive = true,
                    CreatedDate = DateTime.Now
                },
                new User
                {
                    UserId = 2,
                    FullName = "John Employee",
                    Email = "employee@example.com",
                    Password = "emp123", 
                    Role = "Employee",
                    IsActive = true,
                    CreatedDate = DateTime.Now
                }
            );
        }
    }
}