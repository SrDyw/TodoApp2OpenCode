using System.Text.Json.Serialization;

namespace TodoApp2OpenCode.Models;

/// <summary>
/// Modelo que representa el estado completo del tablero de tareas.
/// </summary>
/// <remarks>
/// Contiene todas las columnas y tareas del usuario, permitiendo
/// guardar el estado completo del tablero en una sola operación.
/// </remarks>
public class TodoBoard
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("userId")]
    public string User { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("participantIds")]
    public List<string> ParticipantIds { get; set; } = new();

    [JsonPropertyName("participantNames")]
    public Dictionary<string, string> ParticipantNames { get; set; } = new();

    [JsonPropertyName("ownerName")]
    public string OwnerName { get; set; } = string.Empty;

    /// <summary>
    /// Lista de columnas en el tablero.
    /// </summary>
    [JsonPropertyName("columns")]
    public List<TodoColumn> Columns { get; set; } = new();

    /// <summary>
    /// Lista de tareas en el tablero.
    /// </summary>
    [JsonPropertyName("items")]
    public List<TodoItem> Items { get; set; } = new();
}
