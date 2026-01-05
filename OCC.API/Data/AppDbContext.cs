using Microsoft.EntityFrameworkCore;
using OCC.Shared.Models;

namespace OCC.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectTask> ProjectTasks { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Employee> StaffMembers { get; set; }
        public DbSet<Customer> Customers { get; set; }

        public DbSet<TimeRecord> TimeRecords { get; set; }
        public DbSet<AttendanceRecord> AttendanceRecords { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<AppSetting> AppSettings { get; set; }
        public DbSet<TaskAssignment> TaskAssignments { get; set; }
        public DbSet<TaskComment> TaskComments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships if needed
            // Example:
            // modelBuilder.Entity<ProjectTask>()
            //     .HasOne<Project>()
            //     .WithMany()
            //     .HasForeignKey(t => t.ProjectId);
        }
    }
}
