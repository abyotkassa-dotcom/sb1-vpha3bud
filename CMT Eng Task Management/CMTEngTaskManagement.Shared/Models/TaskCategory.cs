using System.ComponentModel.DataAnnotations;

namespace CMTEngTaskManagement.Shared.Models
{
    public class TaskCategory
    {
        [Key]
        public int CategoryId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        // Navigation properties
        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
        public ICollection<TaskSubType> SubTypes { get; set; } = new List<TaskSubType>();
        public TaskCategoryTargetDays? TargetDays { get; set; }
    }

    public class TaskCategoryTargetDays
    {
        [Key]
        public int TargetId { get; set; }

        public int CategoryId { get; set; }
        public TaskCategory Category { get; set; } = null!;

        public int TargetDays { get; set; } = 5;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class TaskSubType
    {
        [Key]
        public int SubTypeId { get; set; }

        public int CategoryId { get; set; }
        public TaskCategory Category { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        // Navigation properties
        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    }

    public class RequestType
    {
        [Key]
        public int RequestTypeId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        // Navigation properties
        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    }

    public class TaskPriorityLevel
    {
        [Key]
        public int PriorityId { get; set; }

        [Required]
        [MaxLength(50)]
        public string LevelName { get; set; } = string.Empty;

        public int OrderRank { get; set; }

        // Navigation properties
        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    }
}