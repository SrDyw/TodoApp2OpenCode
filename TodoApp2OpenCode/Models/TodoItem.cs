using System.Text.Json.Serialization;

namespace TodoApp2OpenCode.Models;

public class TodoStep
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("isCompleted")]
    public bool IsCompleted { get; set; } = false;
}

public class TodoItem
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("isCompleted")]
    public bool IsCompleted { get; set; } = false;

    [JsonPropertyName("columnId")]
    public string ColumnId { get; set; } = string.Empty;

    [JsonPropertyName("order")]
    public int Order { get; set; } = 0;

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    [JsonPropertyName("priority")]
    public TodoPriority Priority { get; set; } = TodoPriority.Normal;

    [JsonPropertyName("assignedUsers")]
    public Dictionary<string, string> AssignedUsers { get; set; } = new();

    [JsonPropertyName("steps")]
    public List<TodoStep>? Steps { get; set; }

    [JsonPropertyName("currentStepIndex")]
    public int CurrentStepIndex { get; set; } = -1;

    public int CompletedStepsCount => Steps?.Count(s => s.IsCompleted) ?? 0;
    public int TotalStepsCount => Steps?.Count ?? 0;
    public double ProgressPercentage => TotalStepsCount > 0 ? (double)CompletedStepsCount / TotalStepsCount * 100 : 0;
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
