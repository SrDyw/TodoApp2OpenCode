using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Microsoft.JSInterop;
using MudBlazor;
using TodoApp2OpenCode.Components.Pages;
using TodoApp2OpenCode.Constants;
using TodoApp2OpenCode.Data;
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
    private readonly IFlowBoardDbContextFactory _contextFactory;

    public TodoService(IFlowBoardDbContextFactory contextFactory, IJSRuntime jsRuntime, IBoardService boardService, ILogService logService, IAuthService authService)
    {
        _jsRuntime = jsRuntime;
        _boardService = boardService;
        _logService = logService;
        _contextFactory = contextFactory;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };

        _authService = authService;
    }

    private bool CheckPermission(TodoBoard board, Func<BoardPermissions, bool> hasPermission)
    {
        var userId = _authService.CurrentUser?.Id;
        if (string.IsNullOrEmpty(userId))
            return false;

        if (board.User != userId)
        {
            if (!board.ParticipantPermissions.TryGetValue(userId, out var perms) || perms == null)
                return false;

            if (!hasPermission(perms))
                return false;
        }

        return true;
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
    
    public async Task<(string, bool)> AddItemAsync(string boardId, TodoItem item)
    {
        try
        {
            if (_authService.CurrentUser == null)
            {
                return (SystemMessages.OPERATION_AUTH_REQUIRED, false);
            }

            await using var context = await _contextFactory.CreateDbContextAsync();
            var board = await context.Boards.FirstOrDefaultAsync(x => x.Id == boardId);

            if (board == null) 
                return (SystemMessages.DASHBOARD_NOT_EXISTS, false);

            var permCheck = CheckPermission(board, p => p.CanAddTasks);
            if (!permCheck)
                return (SystemMessages.PERMISSION_DENIED, false);

            if (string.IsNullOrEmpty(item.Id))
            {
                item.Id = Guid.NewGuid().ToString();
            }

            var column = await context.Columns
                .FirstOrDefaultAsync(x => x.Id == item.ColumnId);

            if (column == null)
                return (SystemMessages.COLUMN_DOESNT_EXISTS, false);

            var items = await context.Items
                .Where(x => x.ColumnId == column.Id)
                .ToListAsync();

            var maxOrder = items.Count;

            item.TodoBoardId = boardId;
            item.Order = maxOrder;
            item.CreatedAt = DateTime.Now;
            item.UpdatedAt = DateTime.Now;

            var columnName = column.Name;

            await context.Items.AddAsync(item);
            await context.SaveChangesAsync();
            
            await _logService.AddLogAsync(new LogItem
            {
                Action = DatabaseAction.Crear,
                Message = $"Crea tarea '{item.Title}' en columna '{columnName}'",
                BoardId = boardId,
                User = _authService.CurrentUser?.Username ?? "Sistema"
            });

            return (SystemMessages.ITEM_ADDED.Replace(":task", item.Title), true);
        }
        catch
        {
            return (SystemMessages.NETWORK_OR_INTERNAL_ERROR, false);
        }
    }

    public async Task<(string, bool)> UpdateItemAsync(string boardId, TodoItem item)
    {
        try
        {
            if (_authService.CurrentUser == null)
            {
                return (SystemMessages.OPERATION_AUTH_REQUIRED, false);
            }

            await using var context = await _contextFactory.CreateDbContextAsync();
            var board = await context.Boards.FirstOrDefaultAsync(x => x.Id == boardId);
            if (board == null) return (SystemMessages.DASHBOARD_NOT_EXISTS, false);

            
            var dbItem = await context.Items.FirstOrDefaultAsync(x => x.Id == item.Id);
            if (dbItem == null) return (SystemMessages.ITEM_NOT_EXISTS.Replace(":task", item.Title), false);

            var permCheck = CheckPermission(board, p => p.CanModifyTasks);
            if (!permCheck)
                return (SystemMessages.PERMISSION_DENIED, false);


            var stepsDb = await context.Steps
                .Where(x => x.ItemId == item.Id)
                .ToListAsync() ?? [];

            var newSteps = item.Steps?
                .Where(step => stepsDb.Any(stepDb => stepDb.Id == step.Id) == false)
                .ToList() ?? [];
            
            var toRemoveSteps = stepsDb
                .Where(stepDb => item.Steps?.Any(step => stepDb.Id == step.Id) == false)
                .ToList() ?? [];

            if (stepsDb.Count != 0)
            {
                foreach(var stepDb in stepsDb ?? [])
                {
                    var updateStep = item.Steps!.FirstOrDefault(x => x.Id == stepDb.Id);
                    if (updateStep == null) continue;

                    stepDb.Order = updateStep.Order;
                    stepDb.Name = updateStep.Name;
                    stepDb.IsCompleted = updateStep.IsCompleted;
                }
            }


            dbItem.Title = item.Title;
            dbItem.UpdatedAt = item.UpdatedAt;
            dbItem.DueDate = item.DueDate;
            dbItem.AssignedUsers = item.AssignedUsers;
            dbItem.Description = item.Description;
            dbItem.ColumnId = item.ColumnId;
            dbItem.CurrentStepIndex = item.CurrentStepIndex;
            dbItem.StartDate = item.StartDate;
            dbItem.EndDate = item.EndDate;

            context.Steps.RemoveRange(toRemoveSteps);

            await context.Steps.AddRangeAsync(newSteps);
            await context.SaveChangesAsync();

            await _logService.AddLogAsync(new LogItem
            {
                Action = DatabaseAction.Actualizar,
                Message = $"Actualiza tarea '{item.Title}'",
                BoardId = boardId,
                User = _authService.CurrentUser?.Username ?? "Sistema"
            });
            
            return (SystemMessages.ITEM_ADDED.Replace(":task", item.Title), true);
        }
        catch
        {
            return (SystemMessages.NETWORK_OR_INTERNAL_ERROR, false);
        }
    }

    public async Task<(string, bool)> DeleteItemAsync(string boardId, string itemId)
    {
        try
        {
            if (_authService.CurrentUser == null)
                return (SystemMessages.OPERATION_AUTH_REQUIRED, false);
            await using var context = await _contextFactory.CreateDbContextAsync();
            var board = await context.Boards.FirstOrDefaultAsync(x => x.Id == boardId);
            if (board == null) return (SystemMessages.DASHBOARD_NOT_EXISTS, false);

            if (!CheckPermission(board, p => p.CanDeleteTasks))
                return (SystemMessages.PERMISSION_DENIED, false);

            var item = await context.Items.FirstOrDefaultAsync(x => x.Id == itemId);
            if (item == null) return (SystemMessages.ITEM_NOT_EXISTS, false);

            context.Items.Remove(item);

            await context.SaveChangesAsync();
            await _logService.AddLogAsync(new LogItem
            {
                BoardId = boardId,
                Action = DatabaseAction.Remover,
                Message = $"Remueve tarea {item.Title} del tablero {board.Name}",
                User = _authService.CurrentUser!.Username
            });
            return (SystemMessages.ITEM_REMOVED, true);
        }
        catch
        {
            return (SystemMessages.NETWORK_OR_INTERNAL_ERROR, false);
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
            await using var context = await _contextFactory.CreateDbContextAsync();
            var board = await context.Boards
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == boardId);

            if (board == null) return (null, SystemMessages.DASHBOARD_NOT_EXISTS);
            if (await context.Columns.AnyAsync(x => x.Id == targetColumnId) == false)
                return (null, SystemMessages.COLUMN_DOESNT_EXISTS);

            var permCheck = CheckPermission(board, p => p.CanModifyTasks);
            if (!permCheck)
                return (null, SystemMessages.PERMISSION_DENIED);

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
                if (targetIndexInColumn != -1 && targetIndexInColumn < columnItems.Count && columnItems.Count > 1)
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
            await context.SaveChangesAsync();
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

}
