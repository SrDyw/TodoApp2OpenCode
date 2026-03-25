using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TodoApp2OpenCode.Models;

[Table("Steps")]
public class TodoStep
{
    [Key]
    [MaxLength(50)]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public bool IsCompleted { get; set; } = false;

    [MaxLength(50)]
    public string ItemId { get; set; } = string.Empty;
}

[Table("Items")]
public class TodoItem
{
    [Key]
    [MaxLength(50)]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public bool IsCompleted { get; set; } = false;

    [MaxLength(50)]
    public string ColumnId { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? TodoBoardId { get; set; }

    public int Order { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public TodoPriority Priority { get; set; } = TodoPriority.Normal;

    public List<TodoStep>? Steps { get; set; }

    [NotMapped]
    public Dictionary<string, string> AssignedUsers { get; set; } = new();

    public string AssignedUsersJson
    {
        get => System.Text.Json.JsonSerializer.Serialize(AssignedUsers);
        set => AssignedUsers = string.IsNullOrEmpty(value) 
            ? new Dictionary<string, string>() 
            : System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(value) ?? new();
    }

    public int CurrentStepIndex { get; set; } = -1;

    [NotMapped]
    public int CompletedStepsCount => Steps?.Count(s => s.IsCompleted) ?? 0;

    [NotMapped]
    public int TotalStepsCount => Steps?.Count ?? 0;

    [NotMapped]
    public double ProgressPercentage => TotalStepsCount > 0 ? (double)CompletedStepsCount / TotalStepsCount * 100 : 0;

    [NotMapped]
    public string CurrentStepName 
    { 
        get 
        {
            if (Steps == null || Steps.Count == 0) return string.Empty;
            var nextStep = Steps.FirstOrDefault(s => !s.IsCompleted);
            return nextStep?.Name ?? string.Empty;
        }
    }
}

public enum TodoPriority
{
    Low = 0,
    Normal = 1,
    High = 2
}
