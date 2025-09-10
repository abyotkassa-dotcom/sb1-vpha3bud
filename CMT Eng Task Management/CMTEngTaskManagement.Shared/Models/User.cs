using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMTEngTaskManagement.Shared.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public UserRole Role { get; set; }

        public UserStatus Status { get; set; } = UserStatus.Active;

        [MaxLength(255)]
        public string? ProfilePicturePath { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public int? SupervisorId { get; set; }
        [ForeignKey(nameof(SupervisorId))]
        public User? Supervisor { get; set; }

        public int? ShopId { get; set; }
        [ForeignKey(nameof(ShopId))]
        public Shop? Shop { get; set; }

        [MaxLength(255)]
        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetExpiresAt { get; set; }

        // Navigation properties
        public ICollection<TaskItem> CreatedTasks { get; set; } = new List<TaskItem>();
        public ICollection<TaskComment> Comments { get; set; } = new List<TaskComment>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public ICollection<User> Subordinates { get; set; } = new List<User>();
    }

    public enum UserRole
    {
        TeamLeader,
        Director,
        Engineer,
        CustomerPersonnel,
        Customer,
        ShopTL
    }

    public enum UserStatus
    {
        Active,
        Inactive,
        Suspended
    }
}