using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TodoApp2OpenCode.Models;

[Table("Boards")]
public class TodoBoard
{
    [Key]
    [MaxLength(50)]
    public string Id { get; set; } = string.Empty;

    [MaxLength(50)]
    public string User { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [NotMapped]
    public Dictionary<string, string> Participants { get; set; } = new();

    [MaxLength(100)]
    public string OwnerName { get; set; } = string.Empty;

    public List<TodoColumn> Columns { get; set; } = new();

    public List<TodoItem> Items { get; set; } = new();

    public List<CalendarEvent> Events { get; set; } = new();

    public string ParticipantsJson
    {
        get => System.Text.Json.JsonSerializer.Serialize(Participants);
        set => Participants = string.IsNullOrEmpty(value) 
            ? new Dictionary<string, string>() 
            : System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(value) ?? new();
    }
}
