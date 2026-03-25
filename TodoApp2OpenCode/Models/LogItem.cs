using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TodoApp2OpenCode.Models;

[Table("LogItems")]
public class LogItem
{
    [Key]
    public long Id { get; set; }
    
    public DatabaseAction Action { get; set; }
    
    [MaxLength(100)]
    public string User { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string Message { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    [MaxLength(50)]
    public string BoardId { get; set; } = string.Empty;
}
