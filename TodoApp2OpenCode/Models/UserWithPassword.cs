using System.Text.Json.Serialization;

namespace TodoApp2OpenCode.Models;

public class UserWithPassword : User
{

    [JsonPropertyName("passwordHash")]
    public string PasswordHash { get; set; } = string.Empty;

}

public class User
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}