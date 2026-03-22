using System.Text.Json;
using Microsoft.JSInterop;
using TodoApp2OpenCode.Models;

namespace TodoApp2OpenCode.Services;

/// <summary>
/// Servicio para gestionar el tablero de tareas usando localStorage.
/// </summary>
/// <remarks>
/// Proporciona operaciones CRUD completas para columnas y tareas,
/// manteniendo la persistencia en el navegador del usuario.
/// </remarks>
public class TodoService
{
    // Clave usada para almacenar el tablero en localStorage
    private const string STORAGE_KEY = "todoboard_data";

    // Referencia a JavaScript Interop para acceder a localStorage
    private readonly IJSRuntime _jsRuntime;

    // Instancia de JsonSerializerOptions para la serialización
    private readonly JsonSerializerOptions _jsonOptions;

    // Caché local del tablero para reducir accesos a localStorage
    private TodoBoard? _cachedBoard;

    /// <summary>
    /// Constructor del servicio de tablero de tareas.
    /// </summary>
    /// <param name="jsRuntime">Instancia de IJSRuntime para acceder a localStorage.</param>
    public TodoService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };
    }

    // ============================================
    // MÉTODOS DE CARGA DE DATOS
    // ============================================

    /// <summary>
    /// Obtiene el tablero completo desde localStorage.
    /// </summary>
    /// <returns>El tablero con columnas y tareas.</returns>
    public async Task<TodoBoard> GetBoardAsync()
    {
        // Retornamos caché si existe
        if (_cachedBoard != null)
        {
            return _cachedBoard;
        }

        try
        {
            var json = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", STORAGE_KEY);

            if (string.IsNullOrEmpty(json))
            {
                // Si no hay datos, inicializamos con tablero vacío
                _cachedBoard = new TodoBoard();
            }
            else
            {
                _cachedBoard = JsonSerializer.Deserialize<TodoBoard>(json, _jsonOptions) ?? new TodoBoard();
            }
        }
        catch
        {
            _cachedBoard = new TodoBoard();
        }

        return _cachedBoard;
    }

    /// <summary>
    /// Obtiene todas las columnas en su orden de lista.
    /// </summary>
    public async Task<List<TodoColumn>> GetColumnsAsync()
    {
        var board = await GetBoardAsync();
        return board.Columns.ToList();
    }

    /// <summary>
    /// Obtiene todas las tareas de una columna específica.
    /// </summary>
    /// <param name="columnId">ID de la columna.</param>
    public async Task<List<TodoItem>> GetItemsByColumnAsync(string columnId)
    {
        var board = await GetBoardAsync();
        return board.Items
            .Where(i => i.ColumnId == columnId)
            .OrderBy(i => i.Order)
            .ToList();
    }

    // ============================================
    // MÉTODOS DE COLUMNAS (CRUD)
    // ============================================

    /// <summary>
    /// Agrega una nueva columna al tablero.
    /// </summary>
    /// <param name="name">Nombre de la columna.</param>
    /// <param name="color">Color de la columna (hexadecimal).</param>
    public async Task<TodoColumn?> AddColumnAsync(string name, string color = "#7e6fff")
    {
        try
        {
            var board = await GetBoardAsync();

            var column = new TodoColumn
            {
                Name = name.Trim(),
                Color = color,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            board.Columns.Add(column);
            await SaveBoardAsync(board);

            return column;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Actualiza una columna existente.
    /// </summary>
    /// <param name="column">Columna con los datos actualizados.</param>
    public async Task<bool> UpdateColumnAsync(TodoColumn column)
    {
        try
        {
            var board = await GetBoardAsync();
            var index = board.Columns.FindIndex(c => c.Id == column.Id);

            if (index == -1)
            {
                return false;
            }

            column.UpdatedAt = DateTime.Now;
            board.Columns[index] = column;

            await SaveBoardAsync(board);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Elimina una columna y todas sus tareas asociadas.
    /// </summary>
    /// <param name="columnId">ID de la columna a eliminar.</param>
    public async Task<bool> DeleteColumnAsync(string columnId)
    {
        try
        {
            var board = await GetBoardAsync();

            board.Columns.RemoveAll(c => c.Id == columnId);
            board.Items.RemoveAll(i => i.ColumnId == columnId);

            await SaveBoardAsync(board);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Intercambia dos columnas por sus índices.
    /// </summary>
    /// <param name="fromIndex">Índice de origen.</param>
    /// <param name="toIndex">Índice de destino.</param>
    public async Task<bool> SwapColumnsAsync(int fromIndex, int toIndex)
    {
        try
        {
            var board = await GetBoardAsync();

            if (fromIndex < 0 || fromIndex >= board.Columns.Count ||
                toIndex < 0 || toIndex >= board.Columns.Count)
            {
                return false;
            }

            (board.Columns[fromIndex], board.Columns[toIndex]) = (board.Columns[toIndex], board.Columns[fromIndex]);

            await SaveBoardAsync(board);
            return true;
        }
        catch
        {
            return false;
        }
    }

    // ============================================
    // MÉTODOS DE TAREAS (CRUD)
    // ============================================

    /// <summary>
    /// Agrega una nueva tarea a una columna.
    /// </summary>
    /// <param name="item">Tarea a agregar.</param>
    public async Task<bool> AddItemAsync(TodoItem item)
    {
        try
        {
            var board = await GetBoardAsync();

            if (string.IsNullOrEmpty(item.Id))
            {
                item.Id = Guid.NewGuid().ToString();
            }

            // Calculamos el orden como el último en la columna
            var maxOrder = board.Items
                .Where(i => i.ColumnId == item.ColumnId)
                .Select(i => i.Order)
                .DefaultIfEmpty(-1)
                .Max();

            item.Order = maxOrder + 1;
            item.CreatedAt = DateTime.Now;
            item.UpdatedAt = DateTime.Now;

            board.Items.Add(item);
            await SaveBoardAsync(board);

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Actualiza una tarea existente.
    /// </summary>
    /// <param name="item">Tarea con los datos actualizados.</param>
    public async Task<bool> UpdateItemAsync(TodoItem item)
    {
        try
        {
            var board = await GetBoardAsync();
            var index = board.Items.FindIndex(i => i.Id == item.Id);

            if (index == -1)
            {
                return false;
            }

            item.UpdatedAt = DateTime.Now;
            board.Items[index] = item;

            await SaveBoardAsync(board);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Elimina una tarea del tablero.
    /// </summary>
    /// <param name="itemId">ID de la tarea a eliminar.</param>
    public async Task<bool> DeleteItemAsync(string itemId)
    {
        try
        {
            var board = await GetBoardAsync();
            board.Items.RemoveAll(i => i.Id == itemId);

            await SaveBoardAsync(board);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Mueve una tarea a otra columna.
    /// </summary>
    /// <param name="itemId">ID de la tarea.</param>
    /// <param name="targetColumnId">ID de la columna destino.</param>
    public async Task<bool> MoveItemToColumnAsync(string itemId, string targetColumnId)
    {
        try
        {
            var board = await GetBoardAsync();
            var item = board.Items.FirstOrDefault(i => i.Id == itemId);

            if (item == null)
            {
                return false;
            }

            // Calculamos el orden en la columna destino
            var maxOrder = board.Items
                .Where(i => i.ColumnId == targetColumnId)
                .Select(i => i.Order)
                .DefaultIfEmpty(-1)
                .Max();

            item.ColumnId = targetColumnId;
            item.Order = maxOrder + 1;
            item.UpdatedAt = DateTime.Now;

            await SaveBoardAsync(board);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Alterna el estado de completado de una tarea.
    /// </summary>
    /// <param name="itemId">ID de la tarea.</param>
    public async Task<bool> ToggleCompleteAsync(string itemId)
    {
        try
        {
            var board = await GetBoardAsync();
            var item = board.Items.FirstOrDefault(i => i.Id == itemId);

            if (item == null)
            {
                return false;
            }

            item.IsCompleted = !item.IsCompleted;
            item.UpdatedAt = DateTime.Now;

            await SaveBoardAsync(board);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Reordena una tarea dentro de una columna.
    /// </summary>
    /// <param name="itemId">ID de la tarea.</param>
    /// <param name="newOrder">Nueva posición en la columna.</param>
    public async Task<bool> ReorderItemAsync(string itemId, int newOrder)
    {
        try
        {
            var board = await GetBoardAsync();
            var item = board.Items.FirstOrDefault(i => i.Id == itemId);

            if (item == null)
            {
                return false;
            }

            var oldOrder = item.Order;
            var columnItems = board.Items
                .Where(i => i.ColumnId == item.ColumnId)
                .OrderBy(i => i.Order)
                .ToList();

            // Actualizamos el orden de los items afectados
            foreach (var i in columnItems)
            {
                if (i.Id == itemId)
                {
                    i.Order = newOrder;
                }
                else if (oldOrder < newOrder)
                {
                    // Moviendo hacia abajo: items entre el orden viejo y nuevo bajan
                    if (i.Order > oldOrder && i.Order <= newOrder)
                    {
                        i.Order--;
                    }
                }
                else
                {
                    // Moviendo hacia arriba: items entre el orden nuevo y viejo suben
                    if (i.Order >= newOrder && i.Order < oldOrder)
                    {
                        i.Order++;
                    }
                }
                i.UpdatedAt = DateTime.Now;
            }

            await SaveBoardAsync(board);
            return true;
        }
        catch
        {
            return false;
        }
    }

    // ============================================
    // MÉTODOS DE LIMPIEZA
    // ============================================

    /// <summary>
    /// Limpia la caché local.
    /// </summary>
    public void ClearCache()
    {
        _cachedBoard = null;
    }

    /// <summary>
    /// Elimina todas las columnas y tareas.
    /// </summary>
    public async Task<bool> ClearAllAsync()
    {
        try
        {
            _cachedBoard = new TodoBoard();
            await SaveBoardAsync(_cachedBoard);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Persiste el tablero en localStorage.
    /// </summary>
    private async Task SaveBoardAsync(TodoBoard board)
    {
        var json = JsonSerializer.Serialize(board, _jsonOptions);
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", STORAGE_KEY, json);
        _cachedBoard = board;
    }

    // ============================================
    // ESTADÍSTICAS
    // ============================================

    /// <summary>
    /// Obtiene estadísticas del tablero.
    /// </summary>
    public async Task<Dictionary<string, int>> GetStatisticsAsync()
    {
        var board = await GetBoardAsync();

        return new Dictionary<string, int>
        {
            ["totalColumns"] = board.Columns.Count,
            ["totalItems"] = board.Items.Count,
            ["completedItems"] = board.Items.Count(i => i.IsCompleted),
            ["pendingItems"] = board.Items.Count(i => !i.IsCompleted),
            ["highPriorityItems"] = board.Items.Count(i => i.Priority == TodoPriority.High && !i.IsCompleted)
        };
    }
}
