using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TodoApp2OpenCode.Models;

[Table("UserProfileImages")]
public class UserProfileImage
{
    [Key]
    [MaxLength(50)]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    [MaxLength(50)]
    public string UserId { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? ImageBase64 { get; set; }

    public string? ImageBase64Clob { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
