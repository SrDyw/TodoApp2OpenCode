using System.Text.Json.Serialization;

namespace TodoApp2OpenCode.Models;

/// <summary>
/// Modelo que representa una tarea en el sistema de Todo App.
/// </summary>
/// <remarks>
/// Esta clase contiene todas las propiedades necesarias para gestionar una tarea,
/// incluyendo su identificador único, título, descripción, estado de completado
/// y timestamps de creación/modificación.
/// </remarks>
public class TodoItem
{
    /// <summary>
    /// Identificador único de la tarea.
    /// </summary>
    /// <remarks>
    /// Se genera automáticamente usando un GUID para garantizar unicidad.
    /// </remarks>
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Título de la tarea.
    /// </summary>
    /// <remarks>
    /// Campo obligatorio que describe brevemente la tarea a realizar.
    /// </remarks>
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Descripción detallada de la tarea (opcional).
    /// </summary>
    /// <remarks>
    /// Proporciona información adicional sobre la tarea. Puede estar vacía.
    /// </remarks>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Indica si la tarea ha sido completada.
    /// </summary>
    /// <remarks>
    /// Por defecto es false. Se actualiza cuando el usuario marca la tarea como completada.
    /// </remarks>
    [JsonPropertyName("isCompleted")]
    public bool IsCompleted { get; set; } = false;

    /// <summary>
    /// Identificador de la columna a la que pertenece la tarea.
    /// </summary>
    /// <remarks>
    /// Cada tarea pertenece a una columna específica en el tablero.
    /// </remarks>
    [JsonPropertyName("columnId")]
    public string ColumnId { get; set; } = string.Empty;

    /// <summary>
    /// Posición de la tarea dentro de la columna.
    /// </summary>
    [JsonPropertyName("order")]
    public int Order { get; set; } = 0;

    /// <summary>
    /// Fecha y hora de creación de la tarea.
    /// </summary>
    /// <remarks>
    /// Se establece automáticamente al momento de crear la tarea.
    /// </remarks>
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// Fecha y hora de la última modificación de la tarea.
    /// </summary>
    /// <remarks>
    /// Se actualiza cada vez que se modifica cualquier propiedad de la tarea.
    /// </remarks>
    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// Prioridad de la tarea.
    /// </summary>
    /// <remarks>
    /// Valores: 0=Baja, 1=Normal, 2=Alta. Por defecto es Normal.
    /// </remarks>
    [JsonPropertyName("priority")]
    public TodoPriority Priority { get; set; } = TodoPriority.Normal;
}

/// <summary>
/// Enumera los niveles de prioridad disponibles para las tareas.
/// </summary>
public enum TodoPriority
{
    /// <summary>
    /// Prioridad baja - tareas que pueden esperar.
    /// </summary>
    Low = 0,

    /// <summary>
    /// Prioridad normal - tareas estándar.
    /// </summary>
    Normal = 1,

    /// <summary>
    /// Prioridad alta - tareas urgentes.
    /// </summary>
    High = 2
}
