using System.Text.Json;
using Microsoft.JSInterop;
using MudBlazor;
using TodoApp2OpenCode.Constants;
using TodoApp2OpenCode.Models;
using static MudBlazor.CategoryTypes;

namespace TodoApp2OpenCode.Services;

public class TodoService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly IBoardService _boardService;
    private readonly ILogService _logService;
    private const string BOARDS_KEY = "flowboard_boards";

    private readonly IAuthService _authService;

    private readonly JsonSerializerOptions _jsonOptions;

    public TodoService(IJSRuntime jsRuntime, IBoardService boardService, ILogService logService, IAuthService authService)
    {
        _jsRuntime = jsRuntime;
        _boardService = boardService;
        _logService = logService;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };

        _authService = authService;
    }

    public async Task<List<TodoColumn>> GetColumnsAsync(string boardId)
    {
        var board = await _boardService.GetBoardAsync(boardId);
        return board?.Columns ?? new List<TodoColumn>();
    }

    public async Task<List<TodoItem>> GetItemsByColumnAsync(string boardId, string columnId)
    {
        var board = await _boardService.GetBoardAsync(boardId);
        if (board == null) return new List<TodoItem>();
        return board.Items.Where(i => i.ColumnId == columnId).OrderBy(i => i.Order).ToList();
    }

    public async Task<TodoColumn?> AddColumnAsync(string boardId, string name, string color = "#7e6fff")
    {
        try
        {
            var board = await _boardService.GetBoardAsync(boardId);
            if (board == null) return null;

            var maxOrder = board.Columns.Any() ? board.Columns.Max(c => c.Order) : -1;

            var column = new TodoColumn
            {
                Id = Guid.NewGuid().ToString(),
                Name = name.Trim(),
                Color = color,
                Order = maxOrder + 1,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            board.Columns.Add(column);
            await _boardService.UpdateBoardAsync(board);
            await _logService.AddLogAsync(new LogItem
            {
                Message = $"Añade columna {name} al tablero {board.Name}",
                Action = DatabaseAction.Crear,
                BoardId = boardId,
                User = _authService.CurrentUser!.Username
            });

            return column;
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> UpdateColumnAsync(string boardId, TodoColumn column)
    {
        try
        {
            var board = await _boardService.GetBoardAsync(boardId);
            if (board == null) return false;

            var index = board.Columns.FindIndex(c => c.Id == column.Id);
            if (index == -1) return false;

            column.UpdatedAt = DateTime.Now;
            board.Columns[index] = column;

            await _boardService.UpdateBoardAsync(board);
            
            await _logService.AddLogAsync(new LogItem
            {
                Action = DatabaseAction.Actualizar,
                Message = $"Actualiza columna '{column.Name}'",
                BoardId = boardId,
                User = _authService.CurrentUser?.Username ?? "Sistema"
            });
            
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DeleteColumnAsync(string boardId, string columnId)
    {
        try
        {
            var board = await _boardService.GetBoardAsync(boardId);
            if (board == null) return false;

            var column = board.Columns.FirstOrDefault(x => x.Id == columnId);
            if (column == null) return false;

            board.Columns.RemoveAll(c => c.Id == columnId);
            board.Items.RemoveAll(i => i.ColumnId == columnId);


            await _boardService.UpdateBoardAsync(board);
            await _logService.AddLogAsync(new LogItem
            {
                Action = DatabaseAction.Remover,
                Message = $"Remueve columna {column.Name}",
                BoardId = boardId,
                User = _authService.CurrentUser!.Username
            });
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> SwapColumnsAsync(string boardId, int fromIndex, int toIndex)
    {
        try
        {
            var board = await _boardService.GetBoardAsync(boardId);
            if (board == null) return false;

            if (fromIndex < 0 || fromIndex >= board.Columns.Count ||
                toIndex < 0 || toIndex >= board.Columns.Count)
            {
                return false;
            }

            var columns = board.Columns.OrderBy(c => c.Order).ToList();
            (columns[fromIndex], columns[toIndex]) = (columns[toIndex], columns[fromIndex]);

            for (int i = 0; i < columns.Count; i++)
            {
                columns[i].Order = i;
            }
            board.Columns = columns;

            await _boardService.UpdateBoardAsync(board);
            await _logService.AddLogAsync(new LogItem
            {
                Action = DatabaseAction.Actualizar,
                BoardId = boardId,
                Message = $"Mueve columna {board.Columns[toIndex].Name}",
                User = _authService.CurrentUser!.Username
            });
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> AddItemAsync(string boardId, TodoItem item)
    {
        try
        {
            var board = await _boardService.GetBoardAsync(boardId);
            if (board == null) return false;

            if (string.IsNullOrEmpty(item.Id))
            {
                item.Id = Guid.NewGuid().ToString();
            }

            var maxOrder = board.Items
                .Where(i => i.ColumnId == item.ColumnId)
                .Select(i => i.Order)
                .DefaultIfEmpty(-1)
                .Max();

            item.Order = maxOrder + 1;
            item.CreatedAt = DateTime.Now;
            item.UpdatedAt = DateTime.Now;
            
            var columnName = board.Columns.FirstOrDefault(c => c.Id == item.ColumnId)?.Name ?? "desconocida";

            board.Items.Add(item);
            await _boardService.UpdateBoardAsync(board);
            
            await _logService.AddLogAsync(new LogItem
            {
                Action = DatabaseAction.Crear,
                Message = $"Crea tarea '{item.Title}' en columna '{columnName}'",
                BoardId = board.Id,
                User = _authService.CurrentUser?.Username ?? "Sistema"
            });

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> UpdateItemAsync(string boardId, TodoItem item)
    {
        try
        {
            var board = await _boardService.GetBoardAsync(boardId);
            if (board == null) return false;

            var index = board.Items.FindIndex(i => i.Id == item.Id);
            if (index == -1) return false;

            item.UpdatedAt = DateTime.Now;
            board.Items[index] = item;

            await _boardService.UpdateBoardAsync(board);
            
            await _logService.AddLogAsync(new LogItem
            {
                Action = DatabaseAction.Actualizar,
                Message = $"Actualiza tarea '{item.Title}'",
                BoardId = boardId,
                User = _authService.CurrentUser?.Username ?? "Sistema"
            });
            
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DeleteItemAsync(string boardId, string itemId)
    {
        try
        {
            var board = await _boardService.GetBoardAsync(boardId);
            if (board == null) return false;

            var todo = board.Items.FirstOrDefault(x => x.Id == itemId);
            if (todo == null) return false;

            board.Items.RemoveAll(i => i.Id == itemId);

            await _boardService.UpdateBoardAsync(board);
            await _logService.AddLogAsync(new LogItem
            {
                BoardId = boardId,
                Action = DatabaseAction.Remover,
                Message = $"Remueve tarea {todo.Title} del tablero {board.Name}",
                User = _authService.CurrentUser!.Username
            });
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<TodoBoard?> MoveItemToColumnAsync(string boardId, string itemId, string? hoverItemId, string targetColumnId)
    {
        try
        {
            var board = await _boardService.GetBoardAsync(boardId);
            if (board == null) return null;

            var item = board.Items.FirstOrDefault(i => i.Id == itemId);
            if (item == null) return null;

            var column = board.Columns.FirstOrDefault(x => x.Id == targetColumnId);
            if (column == null) return null;

            var originalColumnId = item.ColumnId;

            var columnItems = board.Items
                .Where(x => x.ColumnId == targetColumnId && x.Id != itemId)
                .OrderBy(x => x.Order)
                .ToList();

            if (string.IsNullOrWhiteSpace(hoverItemId))
            {
                item.Order = columnItems.Any() ? columnItems.Max(x => x.Order) + 1 : 0;
                columnItems.Add(item);
            }
            else
            {
                var hoverIndex = columnItems.FindIndex(x => x.Id == hoverItemId);
                if (hoverIndex < 0) hoverIndex = columnItems.Count;
                columnItems.Insert(hoverIndex, item);
            }

            for (int i = 0; i < columnItems.Count; i++)
            {
                columnItems[i].Order = i;
                columnItems[i].ColumnId = targetColumnId;
            }

            item.ColumnId = targetColumnId;
            item.UpdatedAt = DateTime.Now;

            var updatedBoard = await _boardService.UpdateBoardAsync(board);
            await _logService.AddLogAsync(new LogItem
            {
                Action = DatabaseAction.Actualizar,
                Message = $"Mueve tarea {item.Title} para la columna {column.Name}",
                BoardId = boardId,
                User = _authService.CurrentUser!.Username
            });
            return updatedBoard;
        }
        catch
        {
            return null;
        }
    }
    public async Task<(TodoBoard?, string)> MoveItemAsync(string itemId, string boardId,int targetIndexInColumn, string targetColumnId)
    {
        try
        {
            if (_authService.CurrentUser == null)
            {
                return (null, SystemMessages.OPERATION_AUTH_REQUIRED);
            }
            var board = await _boardService.GetBoardAsync(boardId);

            if (board == null) return (null, SystemMessages.DASHBOARD_NOT_EXISTS);
            if (board.User != _authService.CurrentUser.Id)
            {
                var permision = board.ParticipantPermissions[_authService.CurrentUser.Id];
                if (!permision.CanModifyTasks) return (null, SystemMessages.PERMISSION_DENIED);
            }

            var items = board.Items;
            var item = items.First(x => x.Id == itemId);
            var moveInSameColumn = item.ColumnId == targetColumnId;

            var columnItems = items.Where(x => x.ColumnId == targetColumnId)
                .OrderBy(x => x.Order)
                .ToList();

            var originalColumnItems = moveInSameColumn ? [] :
                items
                    .Where(x => x.ColumnId == item.ColumnId)
                    .OrderBy(x => x.Order)
                    .ToList();

            UpdateOrderOf(columnItems);
            UpdateOrderOf(originalColumnItems);


            TodoItem? hoveredItem = null;
            var originIndex = item.Order;
            var msg = "Tarea movida con éxito";
            if (columnItems.Count > 0)
            {
                if (targetIndexInColumn < columnItems.Count && columnItems.Count > 1)
                {
                    hoveredItem = columnItems[targetIndexInColumn];

                    var targetIndex = hoveredItem.Order;

                    // Solo eliminar de la lista si pertenece a la misma columna, para que al hacer el intercambio no se duplique
                    if (moveInSameColumn)
                    {
                        columnItems.RemoveAt(originIndex);
                    }

                    columnItems.Insert(targetIndex, item);

                }
                else
                {
                    item.Order = columnItems.Count; // se inserta de ultimo 
                    columnItems.Add(item);
                }
            }
            else
            {
                columnItems.Add(item);
            }

            if (!moveInSameColumn)
            {
                originalColumnItems.RemoveAt(originIndex);
            }
            item.ColumnId = targetColumnId;

            UpdateOrderOf(columnItems);
            UpdateOrderOf(originalColumnItems);

            UpdateItemValues(items, columnItems);
            UpdateItemValues(items, originalColumnItems);

            item.ColumnId = targetColumnId;

            if (await _boardService.UpdateBoardAsync(board) != null)
            {
                var column = board.Columns.FirstOrDefault(x => x.Id == targetColumnId);
                var rightMessage = column != null ? $"para la columna {column.Name}" : string.Empty;

                await _logService.AddLogAsync(new LogItem
                {
                    Action = DatabaseAction.Actualizar,
                    Message = $"Mueve tarea {item.Title} {rightMessage}",
                    BoardId = boardId,
                    User = _authService.CurrentUser!.Username
                });
                return (board, msg);
            }
            throw new Exception();
        }
        catch
        {
            return (null, "Ha ocurrido un error al intentar actualizar el tablero");
        }

        static void UpdateOrderOf(List<TodoItem> columnItems)
        {
            for (int i = 0; i < columnItems.Count; i++)
            {
                columnItems[i].Order = i;
            }
        }

        static void UpdateItemValues(List<TodoItem> items, List<TodoItem> source)
        {
            source.ForEach(updateItem =>
            {
                var index = items.IndexOf(items.FirstOrDefault(originalItem => originalItem.Id == updateItem.Id) ?? new());
                if (index != -1)
                {
                    items[index] = updateItem;
                }
            });
        }
    }
    public async Task<Dictionary<string, List<TodoItem>>> GetAllItemsByColumnAsync(string boardId)
    {
        var board = await _boardService.GetBoardAsync(boardId);
        if (board == null) return new Dictionary<string, List<TodoItem>>();

        return board.Items
            .GroupBy(i => i.ColumnId)
            .ToDictionary(g => g.Key, g => g.OrderBy(i => i.Order).ToList());
    }

    public void ClearCache()
    {
        _boardService.ClearCache();
    }

    public async Task<bool> UpdateItemDueDateAsync(string boardId, string itemId, DateTime dueDate)
    {
        try
        {
            var board = await _boardService.GetBoardAsync(boardId);
            if (board == null) return false;

            var item = board.Items.FirstOrDefault(i => i.Id == itemId);
            if (item == null) return false;

            item.DueDate = dueDate;
            item.UpdatedAt = DateTime.Now;

            await _boardService.UpdateBoardAsync(board);
            
            await _logService.AddLogAsync(new LogItem
            {
                Action = DatabaseAction.Actualizar,
                Message = $"Actualiza fecha límite de '{item.Title}' a {dueDate:dd/MM/yyyy}",
                BoardId = boardId,
                User = _authService.CurrentUser?.Username ?? "Sistema"
            });
            
            return true;
        }
        catch
        {
            return false;
        }
    }
}
