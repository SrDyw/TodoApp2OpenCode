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

    public List<string> ParticipantIds { get; set; } = new();

    [NotMapped]
    public Dictionary<string, string> ParticipantNames { get; set; } = new();

    [MaxLength(100)]
    public string OwnerName { get; set; } = string.Empty;

    public List<TodoColumn> Columns { get; set; } = new();

    public List<TodoItem> Items { get; set; } = new();

    [NotMapped]
    public string ParticipantNamesJson
    {
        get => System.Text.Json.JsonSerializer.Serialize(ParticipantNames);
        set => ParticipantNames = string.IsNullOrEmpty(value) 
            ? new Dictionary<string, string>() 
            : System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(value) ?? new();
    }
}
