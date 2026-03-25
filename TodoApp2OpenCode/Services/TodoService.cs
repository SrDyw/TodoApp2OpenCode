using System.Text.Json;
using Microsoft.JSInterop;
using TodoApp2OpenCode.Models;

namespace TodoApp2OpenCode.Services;

public class TodoService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly BoardService _boardService;
    private readonly LogService _logService;
    private const string BOARDS_KEY = "flowboard_boards";

    private readonly AuthService _authService;

    private readonly JsonSerializerOptions _jsonOptions;

    public TodoService(IJSRuntime jsRuntime, BoardService boardService, LogService logService, AuthService authService)
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

            var column = new TodoColumn
            {
                Id = Guid.NewGuid().ToString(),
                Name = name.Trim(),
                Color = color,
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

            (board.Columns[fromIndex], board.Columns[toIndex]) = (board.Columns[toIndex], board.Columns[fromIndex]);

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

            board.Items.Add(item);
            await _boardService.UpdateBoardAsync(board);

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

    public async Task<bool> MoveItemToColumnAsync(string boardId, string itemId, string targetColumnId)
    {
        try
        {
            var board = await _boardService.GetBoardAsync(boardId);
            if (board == null) return false;

            var item = board.Items.FirstOrDefault(i => i.Id == itemId);
            if (item == null) return false;

            var column = board.Columns.FirstOrDefault(x => x.Id == targetColumnId);
            if (column == null) return false;

            var maxOrder = board.Items
                .Where(i => i.ColumnId == targetColumnId)
                .Select(i => i.Order)
                .DefaultIfEmpty(-1)
                .Max();

            item.ColumnId = targetColumnId;
            item.Order = maxOrder + 1;
            item.UpdatedAt = DateTime.Now;

            await _boardService.UpdateBoardAsync(board);
            await _logService.AddLogAsync(new LogItem
            {
                Action = DatabaseAction.Actualizar,
                Message = $"Mueve tarea {item.Title} para la columna {column.Name}",
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
}
