using Microsoft.EntityFrameworkCore;
using CMTEngTaskManagement.Shared.Models;

namespace CMTEngTaskManagement.Server.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Shop> Shops { get; set; }
        public DbSet<TaskItem> Tasks { get; set; }
        public DbSet<TaskCategory> TaskCategories { get; set; }
        public DbSet<TaskCategoryTargetDays> TaskCategoryTargetDays { get; set; }
        public DbSet<TaskSubType> TaskSubTypes { get; set; }
        public DbSet<RequestType> RequestTypes { get; set; }
        public DbSet<TaskPriorityLevel> TaskPriorityLevels { get; set; }
        public DbSet<TaskComment> TaskComments { get; set; }
        public DbSet<TaskAttachment> TaskAttachments { get; set; }
        public DbSet<TaskTransfer> TaskTransfers { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<PerformanceMetrics> PerformanceMetrics { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure enum conversions
            modelBuilder.Entity<User>()
                .Property(e => e.Role)
                .HasConversion<string>();

            modelBuilder.Entity<User>()
                .Property(e => e.Status)
                .HasConversion<string>();

            modelBuilder.Entity<TaskItem>()
                .Property(e => e.Status)
                .HasConversion<string>();

            modelBuilder.Entity<TaskItem>()
                .Property(e => e.AmendmentStatus)
                .HasConversion<string>();

            modelBuilder.Entity<TaskTransfer>()
                .Property(e => e.Status)
                .HasConversion<string>();

            // Configure unique constraints
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Shop>()
                .HasIndex(s => s.Name)
                .IsUnique();

            modelBuilder.Entity<TaskCategory>()
                .HasIndex(c => c.Name)
                .IsUnique();

            modelBuilder.Entity<RequestType>()
                .HasIndex(r => r.Name)
                .IsUnique();

            modelBuilder.Entity<TaskPriorityLevel>()
                .HasIndex(p => p.OrderRank)
                .IsUnique();

            // Configure relationships
            modelBuilder.Entity<User>()
                .HasOne(u => u.Supervisor)
                .WithMany(u => u.Subordinates)
                .HasForeignKey(u => u.SupervisorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Shop>()
                .HasOne(s => s.TeamLeader)
                .WithMany()
                .HasForeignKey(s => s.TeamLeaderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TaskItem>()
                .HasOne(t => t.Creator)
                .WithMany(u => u.CreatedTasks)
                .HasForeignKey(t => t.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure cascade delete for comments and attachments
            modelBuilder.Entity<TaskComment>()
                .HasOne(c => c.Task)
                .WithMany(t => t.TaskComments)
                .HasForeignKey(c => c.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TaskAttachment>()
                .HasOne(a => a.Task)
                .WithMany(t => t.Attachments)
                .HasForeignKey(a => a.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure composite unique constraint for SubType within Category
            modelBuilder.Entity<TaskSubType>()
                .HasIndex(st => new { st.CategoryId, st.Name })
                .IsUnique();

            // Configure unique constraint for TaskCategoryTargetDays
            modelBuilder.Entity<TaskCategoryTargetDays>()
                .HasIndex(t => t.CategoryId)
                .IsUnique();

            // Seed data will be added via migrations
        }
    }
}