using System.Text.Json.Serialization;

namespace TodoApp2OpenCode.Models;

/// <summary>
/// Modelo que representa una columna/tabla en el tablero de tareas.
/// </summary>
/// <remarks>
/// Cada columna puede contener múltiples tareas y representa una categoría
/// o grupo personalizado por el usuario, similar a Milanote.
/// </remarks>
public class TodoColumn
{
    /// <summary>
    /// Identificador único de la columna.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Nombre/título de la columna.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Color de la columna en formato hexadecimal.
    /// </summary>
    /// <remarks>
    /// Se usa para identificar visualmente las columnas.
    /// </remarks>
    [JsonPropertyName("color")]
    public string Color { get; set; } = "#7e6fff";

    /// <summary>
    /// Fecha y hora de creación de la columna.
    /// </summary>
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// Fecha y hora de la última modificación.
    /// </summary>
    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
