using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMTEngTaskManagement.Shared.Models
{
    public class Shop
    {
        [Key]
        public int ShopId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int? TeamLeaderId { get; set; }
        [ForeignKey(nameof(TeamLeaderId))]
        public User? TeamLeader { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<User> Users { get; set; } = new List<User>();
        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
        public ICollection<TaskTransfer> FromTransfers { get; set; } = new List<TaskTransfer>();
        public ICollection<TaskTransfer> ToTransfers { get; set; } = new List<TaskTransfer>();
    }
}