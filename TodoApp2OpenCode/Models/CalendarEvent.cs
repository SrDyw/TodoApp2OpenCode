using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TodoApp2OpenCode.Models;

[Table("CalendarEvents")]
public class CalendarEvent
{
    [Key]
    [MaxLength(50)]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public DateTime EventDate { get; set; }

    [MaxLength(50)]
    public string TodoBoardId { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    [NotMapped]
    public Dictionary<string, string> Participants { get; set; } = new();

    [MaxLength(2000)]
    public string? ParticipantsJson
    {
        get => System.Text.Json.JsonSerializer.Serialize(Participants);
        set => Participants = string.IsNullOrEmpty(value) 
            ? new Dictionary<string, string>() 
            : System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(value) ?? new();
    }
}
