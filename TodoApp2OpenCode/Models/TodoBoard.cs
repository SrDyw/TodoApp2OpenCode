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
