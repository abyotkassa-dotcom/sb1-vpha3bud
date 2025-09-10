using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CMTEngTaskManagement.Server.Data;
using CMTEngTaskManagement.Shared.DTOs;
using CMTEngTaskManagement.Shared.Models;
using System.Security.Claims;

namespace CMTEngTaskManagement.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TasksController> _logger;

        public TasksController(ApplicationDbContext context, ILogger<TasksController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskDto>>> GetTasks([FromQuery] TaskFilterRequest filter)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var userRole = User.FindFirstValue(ClaimTypes.Role)!;

                var query = _context.Tasks
                    .Include(t => t.Category)
                    .Include(t => t.SubType)
                    .Include(t => t.RequestType)
                    .Include(t => t.Priority)
                    .Include(t => t.Creator)
                    .Include(t => t.AmendmentReviewedByTl)
                    .AsQueryable();

                // Apply role-based filtering
                switch (userRole)
                {
                    case "Customer":
                        query = query.Where(t => t.Status == TaskStatus.Completed);
                        break;
                    case "Engineer":
                        if (filter.ViewCompleted)
                        {
                            query = query.Where(t => t.Status == TaskStatus.Completed && 
                                t.AssignedEngineer.Contains(User.FindFirstValue("FullName")!));
                        }
                        else
                        {
                            query = query.Where(t => t.Status != TaskStatus.Completed && 
                                t.AssignedEngineer.Contains(User.FindFirstValue("FullName")!));
                        }
                        break;
                    case "CustomerPersonnel":
                        if (filter.ViewCompleted)
                        {
                            query = query.Where(t => t.Status == TaskStatus.Completed && t.CreatedBy == currentUserId);
                        }
                        else
                        {
                            query = query.Where(t => t.Status != TaskStatus.Completed && t.CreatedBy == currentUserId);
                        }
                        break;
                    case "ShopTL":
                        // Get team member IDs
                        var teamMemberIds = await _context.Users
                            .Where(u => u.SupervisorId == currentUserId)
                            .Select(u => u.UserId)
                            .ToListAsync();
                        teamMemberIds.Add(currentUserId);

                        if (filter.ViewCompleted)
                        {
                            query = query.Where(t => t.Status == TaskStatus.Completed && teamMemberIds.Contains(t.CreatedBy));
                        }
                        else
                        {
                            query = query.Where(t => t.Status != TaskStatus.Completed && teamMemberIds.Contains(t.CreatedBy));
                        }
                        break;
                    case "Director":
                    case "TeamLeader":
                        if (filter.ShowDuplicates)
                        {
                            query = query.Where(t => t.IsDuplicate);
                        }
                        else if (filter.ViewCompleted)
                        {
                            query = query.Where(t => t.Status == TaskStatus.Completed);
                        }
                        else
                        {
                            query = query.Where(t => t.Status != TaskStatus.Completed || t.AmendmentRequest);
                        }
                        break;
                }

                // Apply search filter
                if (!string.IsNullOrEmpty(filter.Search))
                {
                    var searchLower = filter.Search.ToLower();
                    query = query.Where(t => 
                        (t.SerialNumber != null && t.SerialNumber.ToLower().Contains(searchLower)) ||
                        (t.PartNumber != null && t.PartNumber.ToLower().Contains(searchLower)) ||
                        t.Description.ToLower().Contains(searchLower));
                }

                // Apply status filter
                if (!string.IsNullOrEmpty(filter.Status))
                {
                    if (Enum.TryParse<TaskStatus>(filter.Status.Replace(" ", ""), out var statusEnum))
                    {
                        query = query.Where(t => t.Status == statusEnum);
                    }
                }

                // Apply sorting
                query = filter.SortBy?.ToLower() switch
                {
                    "newest" => query.OrderByDescending(t => t.CreatedAt),
                    "oldest" => query.OrderBy(t => t.CreatedAt),
                    "priority_desc" => query.OrderBy(t => t.Priority.OrderRank).ThenByDescending(t => t.CreatedAt),
                    "priority_asc" => query.OrderByDescending(t => t.Priority.OrderRank).ThenByDescending(t => t.CreatedAt),
                    "estimated_asc" => query.OrderBy(t => t.EstimatedCompletionDate).ThenByDescending(t => t.CreatedAt),
                    "estimated_desc" => query.OrderByDescending(t => t.EstimatedCompletionDate).ThenByDescending(t => t.CreatedAt),
                    _ => query.OrderBy(t => t.Priority.OrderRank).ThenByDescending(t => t.CreatedAt)
                };

                var tasks = await query
                    .Select(t => new TaskDto
                    {
                        TaskId = t.TaskId,
                        SerialNumber = t.SerialNumber,
                        PartNumber = t.PartNumber,
                        PoNumber = t.PoNumber,
                        Description = t.Description,
                        CategoryName = t.Category.Name,
                        SubTypeName = t.SubType != null ? t.SubType.Name : null,
                        RequestTypeName = t.RequestType != null ? t.RequestType.Name : null,
                        Status = t.Status.ToString(),
                        Comments = t.Comments,
                        AssignedEngineer = t.AssignedEngineer,
                        PriorityLevelName = t.Priority.LevelName,
                        EstimatedCompletionDate = t.EstimatedCompletionDate,
                        TargetCompletionDate = t.TargetCompletionDate,
                        ActualCompletionDate = t.ActualCompletionDate,
                        AttachmentPath = t.AttachmentPath,
                        AmendmentRequest = t.AmendmentRequest,
                        AmendmentStatus = t.AmendmentStatus.HasValue ? t.AmendmentStatus.Value.ToString() : null,
                        TlReviewerName = t.AmendmentReviewedByTl != null ? t.AmendmentReviewedByTl.FullName : null,
                        RevisionNotes = t.RevisionNotes,
                        ShowRevisionAlert = t.ShowRevisionAlert,
                        CreatorName = t.Creator.FullName,
                        CreatedAt = t.CreatedAt,
                        UpdatedAt = t.UpdatedAt,
                        IsDuplicate = t.IsDuplicate,
                        DuplicateJustification = t.DuplicateJustification,
                        IsOverdue = t.Status != TaskStatus.Completed && 
                                   t.TargetCompletionDate.HasValue && 
                                   t.TargetCompletionDate.Value < DateTime.UtcNow
                    })
                    .ToListAsync();

                return Ok(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tasks");
                return StatusCode(500, "An error occurred while retrieving tasks.");
            }
        }

        [HttpPost]
        public async Task<ActionResult<TaskDto>> CreateTask([FromBody] CreateTaskRequest request)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var userRole = User.FindFirstValue(ClaimTypes.Role)!;

                // Verify user has permission to create tasks
                if (!new[] { "TeamLeader", "Director", "CustomerPersonnel", "ShopTL" }.Contains(userRole))
                {
                    return Forbid("You don't have permission to create tasks.");
                }

                var task = new TaskItem
                {
                    SerialNumber = request.SerialNumber,
                    PartNumber = request.PartNumber,
                    PoNumber = request.PoNumber,
                    Description = request.Description,
                    CategoryId = request.CategoryId,
                    SubTypeId = request.SubTypeId,
                    RequestTypeId = request.RequestTypeId,
                    Comments = request.Comments,
                    AssignedEngineer = request.AssignedEngineer,
                    PriorityId = request.PriorityId,
                    EstimatedCompletionDate = request.EstimatedCompletionDate,
                    TargetCompletionDate = request.TargetCompletionDate,
                    CreatedBy = currentUserId,
                    IsDuplicate = request.IsDuplicate,
                    DuplicateJustification = request.DuplicateJustification,
                    ShopId = request.ShopId
                };

                // Calculate target completion date if not provided
                if (!task.TargetCompletionDate.HasValue && task.CategoryId > 0)
                {
                    var targetDays = await _context.TaskCategoryTargetDays
                        .Where(t => t.CategoryId == task.CategoryId)
                        .Select(t => t.TargetDays)
                        .FirstOrDefaultAsync();

                    if (targetDays > 0)
                    {
                        task.TargetCompletionDate = task.EstimatedCompletionDate.AddDays(targetDays);
                    }
                }

                _context.Tasks.Add(task);
                await _context.SaveChangesAsync();

                // Create notifications for assigned engineers
                if (task.AssignedEngineer != "Unassigned")
                {
                    var engineerNames = task.AssignedEngineer.Split(',').Select(n => n.Trim()).ToList();
                    var engineers = await _context.Users
                        .Where(u => engineerNames.Contains(u.FullName))
                        .ToListAsync();

                    foreach (var engineer in engineers)
                    {
                        var notification = new Notification
                        {
                            UserId = engineer.UserId,
                            Message = $"New task assigned to you: {task.Description.Substring(0, Math.Min(50, task.Description.Length))}... (ID: TC-{task.TaskId})"
                        };
                        _context.Notifications.Add(notification);
                    }
                }

                await _context.SaveChangesAsync();

                // Return the created task
                var createdTask = await _context.Tasks
                    .Include(t => t.Category)
                    .Include(t => t.SubType)
                    .Include(t => t.RequestType)
                    .Include(t => t.Priority)
                    .Include(t => t.Creator)
                    .FirstAsync(t => t.TaskId == task.TaskId);

                var taskDto = MapTaskToDto(createdTask);
                return CreatedAtAction(nameof(GetTask), new { id = task.TaskId }, taskDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating task");
                return StatusCode(500, "An error occurred while creating the task.");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TaskDto>> GetTask(int id)
        {
            try
            {
                var task = await _context.Tasks
                    .Include(t => t.Category)
                    .Include(t => t.SubType)
                    .Include(t => t.RequestType)
                    .Include(t => t.Priority)
                    .Include(t => t.Creator)
                    .Include(t => t.AmendmentReviewedByTl)
                    .FirstOrDefaultAsync(t => t.TaskId == id);

                if (task == null)
                {
                    return NotFound();
                }

                var taskDto = MapTaskToDto(task);
                return Ok(taskDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving task {TaskId}", id);
                return StatusCode(500, "An error occurred while retrieving the task.");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<TaskDto>> UpdateTask(int id, [FromBody] UpdateTaskRequest request)
        {
            try
            {
                if (id != request.TaskId)
                {
                    return BadRequest("Task ID mismatch.");
                }

                var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var userRole = User.FindFirstValue(ClaimTypes.Role)!;
                var fullName = User.FindFirstValue("FullName")!;

                var task = await _context.Tasks
                    .Include(t => t.Creator)
                    .FirstOrDefaultAsync(t => t.TaskId == id);

                if (task == null)
                {
                    return NotFound();
                }

                // Check permissions
                var canUpdate = userRole switch
                {
                    "TeamLeader" => task.Status != TaskStatus.Completed,
                    "Director" => task.Status != TaskStatus.Completed || 
                                 (task.Status == TaskStatus.Completed && task.AmendmentRequest && 
                                  task.AmendmentStatus == AmendmentStatus.ForwardedToDirector),
                    "Engineer" => task.Status != TaskStatus.Completed && task.AssignedEngineer.Contains(fullName),
                    "CustomerPersonnel" => task.Status != TaskStatus.Completed && task.CreatedBy == currentUserId &&
                                          request.Status != "Completed",
                    _ => false
                };

                if (!canUpdate)
                {
                    return Forbid("You don't have permission to update this task.");
                }

                // Update task properties
                if (Enum.TryParse<TaskStatus>(request.Status.Replace(" ", ""), out var statusEnum))
                {
                    task.Status = statusEnum;
                }

                task.Comments = request.Comments;
                task.UpdatedAt = DateTime.UtcNow;

                if (task.Status == TaskStatus.Completed)
                {
                    task.ActualCompletionDate = DateTime.UtcNow;
                    task.AmendmentRequest = false; // Clear amendment request when completed
                }

                // Only Team Leaders and Directors can update assigned engineer and target date
                if (new[] { "TeamLeader", "Director" }.Contains(userRole))
                {
                    if (!string.IsNullOrEmpty(request.AssignedEngineer))
                    {
                        task.AssignedEngineer = request.AssignedEngineer;
                    }

                    if (request.TargetCompletionDate.HasValue)
                    {
                        task.TargetCompletionDate = request.TargetCompletionDate.Value;
                    }
                }

                // Handle revision notes (Engineers and Team Leaders only)
                if (new[] { "Engineer", "TeamLeader" }.Contains(userRole))
                {
                    task.RevisionNotes = request.RevisionNotes;
                    task.ShowRevisionAlert = request.ShowRevisionAlert;
                }

                await _context.SaveChangesAsync();

                // Create notifications for status changes
                await CreateStatusChangeNotifications(task, currentUserId);

                var updatedTask = await _context.Tasks
                    .Include(t => t.Category)
                    .Include(t => t.SubType)
                    .Include(t => t.RequestType)
                    .Include(t => t.Priority)
                    .Include(t => t.Creator)
                    .Include(t => t.AmendmentReviewedByTl)
                    .FirstAsync(t => t.TaskId == id);

                var taskDto = MapTaskToDto(updatedTask);
                return Ok(taskDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating task {TaskId}", id);
                return StatusCode(500, "An error occurred while updating the task.");
            }
        }

        private TaskDto MapTaskToDto(TaskItem task)
        {
            return new TaskDto
            {
                TaskId = task.TaskId,
                SerialNumber = task.SerialNumber,
                PartNumber = task.PartNumber,
                PoNumber = task.PoNumber,
                Description = task.Description,
                CategoryName = task.Category.Name,
                SubTypeName = task.SubType?.Name,
                RequestTypeName = task.RequestType?.Name,
                Status = task.Status.ToString(),
                Comments = task.Comments,
                AssignedEngineer = task.AssignedEngineer,
                PriorityLevelName = task.Priority.LevelName,
                EstimatedCompletionDate = task.EstimatedCompletionDate,
                TargetCompletionDate = task.TargetCompletionDate,
                ActualCompletionDate = task.ActualCompletionDate,
                AttachmentPath = task.AttachmentPath,
                AmendmentRequest = task.AmendmentRequest,
                AmendmentStatus = task.AmendmentStatus?.ToString(),
                TlReviewerName = task.AmendmentReviewedByTl?.FullName,
                RevisionNotes = task.RevisionNotes,
                ShowRevisionAlert = task.ShowRevisionAlert,
                CreatorName = task.Creator.FullName,
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt,
                IsDuplicate = task.IsDuplicate,
                DuplicateJustification = task.DuplicateJustification,
                IsOverdue = task.Status != TaskStatus.Completed && 
                           task.TargetCompletionDate.HasValue && 
                           task.TargetCompletionDate.Value < DateTime.UtcNow
            };
        }

        private async Task CreateStatusChangeNotifications(TaskItem task, int updatedByUserId)
        {
            var message = $"Task TC-{task.TaskId} status changed to {task.Status} by {User.FindFirstValue("FullName")}.";

            // Notify assigned engineers
            if (task.AssignedEngineer != "Unassigned")
            {
                var engineerNames = task.AssignedEngineer.Split(',').Select(n => n.Trim()).ToList();
                var engineers = await _context.Users
                    .Where(u => engineerNames.Contains(u.FullName) && u.UserId != updatedByUserId)
                    .ToListAsync();

                foreach (var engineer in engineers)
                {
                    var notification = new Notification
                    {
                        UserId = engineer.UserId,
                        Message = message
                    };
                    _context.Notifications.Add(notification);
                }
            }

            // Notify task creator if different from current user
            if (task.CreatedBy != updatedByUserId)
            {
                var notification = new Notification
                {
                    UserId = task.CreatedBy,
                    Message = message
                };
                _context.Notifications.Add(notification);
            }

            await _context.SaveChangesAsync();
        }
    }
}