using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMTEngTaskManagement.Shared.Models
{
    public class TaskItem
    {
        [Key]
        public int TaskId { get; set; }

        [MaxLength(100)]
        public string? SerialNumber { get; set; }

        [MaxLength(100)]
        public string? PartNumber { get; set; }

        [MaxLength(100)]
        public string? PoNumber { get; set; }

        [Required]
        public string Description { get; set; } = string.Empty;

        public int CategoryId { get; set; }
        [ForeignKey(nameof(CategoryId))]
        public TaskCategory Category { get; set; } = null!;

        public int? SubTypeId { get; set; }
        [ForeignKey(nameof(SubTypeId))]
        public TaskSubType? SubType { get; set; }

        public int? RequestTypeId { get; set; }
        [ForeignKey(nameof(RequestTypeId))]
        public RequestType? RequestType { get; set; }

        public TaskStatus Status { get; set; } = TaskStatus.Pending;

        public string? Comments { get; set; }

        [MaxLength(255)]
        public string AssignedEngineer { get; set; } = "Unassigned";

        public int PriorityId { get; set; }
        [ForeignKey(nameof(PriorityId))]
        public TaskPriorityLevel Priority { get; set; } = null!;

        public DateTime EstimatedCompletionDate { get; set; }
        public DateTime? TargetCompletionDate { get; set; }
        public DateTime? ActualCompletionDate { get; set; }

        [MaxLength(255)]
        public string? AttachmentPath { get; set; }

        public bool AmendmentRequest { get; set; } = false;
        public AmendmentStatus? AmendmentStatus { get; set; }
        public int? AmendmentReviewedByTlId { get; set; }
        [ForeignKey(nameof(AmendmentReviewedByTlId))]
        public User? AmendmentReviewedByTl { get; set; }

        public bool IsDuplicate { get; set; } = false;
        public string? DuplicateJustification { get; set; }

        public string? RevisionNotes { get; set; }
        public bool ShowRevisionAlert { get; set; } = false;

        public int? SupervisorId { get; set; }
        [ForeignKey(nameof(SupervisorId))]
        public User? Supervisor { get; set; }

        public int? ShopId { get; set; }
        [ForeignKey(nameof(ShopId))]
        public Shop? Shop { get; set; }

        public int CreatedBy { get; set; }
        [ForeignKey(nameof(CreatedBy))]
        public User Creator { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public bool IsMandatory { get; set; } = false;

        public int? CancelledBy { get; set; }
        [ForeignKey(nameof(CancelledBy))]
        public User? CancelledByUser { get; set; }

        public string? CancellationReason { get; set; }
        public DateTime? CancelledAt { get; set; }

        // Navigation properties
        public ICollection<TaskComment> TaskComments { get; set; } = new List<TaskComment>();
        public ICollection<TaskAttachment> Attachments { get; set; } = new List<TaskAttachment>();
        public ICollection<TaskTransfer> Transfers { get; set; } = new List<TaskTransfer>();
    }

    public enum TaskStatus
    {
        Pending,
        InProgress,
        Completed,
        Blocked,
        OnHold,
        Cancelled
    }

    public enum AmendmentStatus
    {
        PendingTLReview,
        ForwardedToDirector,
        Approved,
        Rejected
    }
}