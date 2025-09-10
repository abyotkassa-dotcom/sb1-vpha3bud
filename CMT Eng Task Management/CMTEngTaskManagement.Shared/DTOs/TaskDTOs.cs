using System.ComponentModel.DataAnnotations;

namespace CMTEngTaskManagement.Shared.DTOs
{
    public class CreateTaskRequest
    {
        [MaxLength(100)]
        public string? SerialNumber { get; set; }

        [Required]
        [MaxLength(100)]
        public string PartNumber { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? PoNumber { get; set; }

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public int CategoryId { get; set; }

        public int? SubTypeId { get; set; }
        public int? RequestTypeId { get; set; }

        public string? Comments { get; set; }

        public string AssignedEngineer { get; set; } = "Unassigned";

        [Required]
        public int PriorityId { get; set; }

        [Required]
        public DateTime EstimatedCompletionDate { get; set; }

        public DateTime? TargetCompletionDate { get; set; }

        public bool IsDuplicate { get; set; } = false;
        public string? DuplicateJustification { get; set; }

        public int? ShopId { get; set; }
    }

    public class UpdateTaskRequest
    {
        [Required]
        public int TaskId { get; set; }

        [Required]
        public string Status { get; set; } = string.Empty;

        public string? Comments { get; set; }

        public string? AssignedEngineer { get; set; }

        public DateTime? TargetCompletionDate { get; set; }

        public string? RevisionNotes { get; set; }
        public bool ShowRevisionAlert { get; set; }
    }

    public class TaskDto
    {
        public int TaskId { get; set; }
        public string? SerialNumber { get; set; }
        public string? PartNumber { get; set; }
        public string? PoNumber { get; set; }
        public string Description { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string? SubTypeName { get; set; }
        public string? RequestTypeName { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Comments { get; set; }
        public string AssignedEngineer { get; set; } = string.Empty;
        public string PriorityLevelName { get; set; } = string.Empty;
        public DateTime EstimatedCompletionDate { get; set; }
        public DateTime? TargetCompletionDate { get; set; }
        public DateTime? ActualCompletionDate { get; set; }
        public string? AttachmentPath { get; set; }
        public bool AmendmentRequest { get; set; }
        public string? AmendmentStatus { get; set; }
        public string? TlReviewerName { get; set; }
        public string? RevisionNotes { get; set; }
        public bool ShowRevisionAlert { get; set; }
        public string CreatorName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsDuplicate { get; set; }
        public string? DuplicateJustification { get; set; }
        public bool IsOverdue { get; set; }
        public int? TargetDays { get; set; }
    }

    public class TaskFilterRequest
    {
        public string? Search { get; set; }
        public string? Status { get; set; }
        public bool ViewCompleted { get; set; } = false;
        public bool ShowDuplicates { get; set; } = false;
        public string? SortBy { get; set; } = "priority_desc";
        public int? UserId { get; set; }
        public bool FilterMyTasks { get; set; } = false;
    }
}