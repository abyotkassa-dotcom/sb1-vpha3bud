using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMTEngTaskManagement.Shared.Models
{
    public class TaskComment
    {
        [Key]
        public int CommentId { get; set; }

        public int TaskId { get; set; }
        [ForeignKey(nameof(TaskId))]
        public TaskItem Task { get; set; } = null!;

        public int UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;

        [Required]
        public string CommentText { get; set; } = string.Empty;

        public bool IsIntervention { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class TaskAttachment
    {
        [Key]
        public int AttachmentId { get; set; }

        public int TaskId { get; set; }
        [ForeignKey(nameof(TaskId))]
        public TaskItem Task { get; set; } = null!;

        [Required]
        [MaxLength(255)]
        public string FilePath { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string FileName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? FileType { get; set; }

        public int? UploadedBy { get; set; }
        [ForeignKey(nameof(UploadedBy))]
        public User? UploadedByUser { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }

    public class TaskTransfer
    {
        [Key]
        public int TransferId { get; set; }

        public int TaskId { get; set; }
        [ForeignKey(nameof(TaskId))]
        public TaskItem Task { get; set; } = null!;

        public int FromShopId { get; set; }
        [ForeignKey(nameof(FromShopId))]
        public Shop FromShop { get; set; } = null!;

        public int ToShopId { get; set; }
        [ForeignKey(nameof(ToShopId))]
        public Shop ToShop { get; set; } = null!;

        public int TransferredByUserId { get; set; }
        [ForeignKey(nameof(TransferredByUserId))]
        public User TransferredByUser { get; set; } = null!;

        public TransferStatus Status { get; set; } = TransferStatus.Pending;

        public string? Comments { get; set; }

        public int? ActionByUserId { get; set; }
        [ForeignKey(nameof(ActionByUserId))]
        public User? ActionByUser { get; set; }

        public DateTime? ActionAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public enum TransferStatus
    {
        Pending,
        Accepted,
        Rejected,
        Cancelled
    }
}