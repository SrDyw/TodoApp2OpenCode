using Microsoft.EntityFrameworkCore;
using TodoApp2OpenCode.Data;
using TodoApp2OpenCode.Models;

namespace TodoApp2OpenCode.Services;

public class BoardService : IBoardService
{
    private readonly IDbContextFactory<FlowBoardDbContext> _contextFactory;
    private readonly IBoardNotifier _notifier;
    private readonly IAuthService _authService;
    private readonly ILogService _logService;

    public BoardService(IDbContextFactory<FlowBoardDbContext> contextFactory, IBoardNotifier notifier, IAuthService authService, ILogService logService)
    {
        _contextFactory = contextFactory;
        _notifier = notifier;
        _authService = authService;
        _logService = logService;
    }

    public async Task<List<TodoBoard>> GetUserBoardsAsync(string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var boards = await context.Boards
            .AsNoTracking()
            .Include(b => b.Columns)
            .Include(b => b.Events)
            .ToListAsync();
        
        var boardIds = boards.Where(b => b.User == userId || b.Participants.ContainsKey(userId)).Select(b => b.Id).ToList();
        
        var items = await context.Items
            .AsNoTracking()
            .Where(i => boardIds.Contains(i.TodoBoardId))
            .ToListAsync();
        
        var result = boards
            .Where(b => b.User == userId || b.Participants.ContainsKey(userId))
            .OrderBy(b => b.Name)
            .ToList();
        
        foreach (var board in result)
        {
            board.Items = items.Where(i => i.TodoBoardId == board.Id).ToList();
        }
        
        return result;
    }

    public async Task<TodoBoard?> GetBoardAsync(string boardId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var board = await context.Boards
            .AsNoTracking()
            .Include(b => b.Columns.OrderBy(c => c.Order))
            .Include(b => b.Events)
            .FirstOrDefaultAsync(b => b.Id == boardId);

        if (board != null)
        {
            board.Columns = board.Columns.OrderBy(c => c.Order).ToList();

            var items = await context.Items
                .AsNoTracking()
                .Include(i => i.Steps)
                .Where(i => i.TodoBoardId == boardId)
                .ToListAsync();
            
            foreach (var item in items)
            {
                if (!string.IsNullOrEmpty(item.AssignedUsersJson))
                {
                    item.AssignedUsers = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(item.AssignedUsersJson) ?? new();
                }
            }
            
            board.Items = items;
        }

        return board;
    }

    public async Task<TodoBoard?> CreateBoardAsync(string userId, string name, string? description = null, List<(string Id, string Name)>? participants = null)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            var participantDict = participants?.ToDictionary(p => p.Id, p => p.Name) ?? new Dictionary<string, string>();

            var board = new TodoBoard
            {
                Id = Guid.NewGuid().ToString(),
                User = userId,
                Name = name,
                Description = description ?? string.Empty,
                Participants = participantDict,
                OwnerName = string.Empty,
                Columns = new List<TodoColumn>(),
                Items = new List<TodoItem>()
            };

            context.Boards.Add(board);
            await context.SaveChangesAsync();
            
            await _notifier.NotifyBoardUpdatedAsync(board.Id, _authService.CurrentUser?.Id);
            
            return board;
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> AddParticipantAsync(string boardId, string userId, string userName)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var board = await context.Boards.FindAsync(boardId);
            if (board == null) return false;

            if (!board.Participants.ContainsKey(userId))
            {
                board.Participants[userId] = userName;
                await context.SaveChangesAsync();
                
                await _notifier.NotifyBoardUpdatedAsync(boardId, _authService.CurrentUser?.Id);
            }
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> RemoveParticipantAsync(string boardId, string userId)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var board = await context.Boards.FindAsync(boardId);
            if (board == null) return false;

            board.Participants.Remove(userId);

            await context.SaveChangesAsync();
            
            await _notifier.NotifyBoardUpdatedAsync(boardId, _authService.CurrentUser?.Id);
            
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> UpdateBoardAsync(TodoBoard board)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            var existingBoard = await context.Boards
                .Include(b => b.Columns)
                .Include(b => b.Items)
                    .ThenInclude(i => i.Steps)
                .FirstOrDefaultAsync(b => b.Id == board.Id);

            if (existingBoard == null) return false;

            existingBoard.Name = board.Name;
            existingBoard.Description = board.Description;
            existingBoard.Participants = board.Participants;
            existingBoard.OwnerName = board.OwnerName;

            var existingColumns = existingBoard.Columns.ToList();
            var existingItems = existingBoard.Items.ToList();

            foreach (var column in board.Columns)
            {
                var existingColumn = existingColumns.FirstOrDefault(c => c.Id == column.Id);
                if (existingColumn != null)
                {
                    existingColumn.Name = column.Name;
                    existingColumn.Color = column.Color;
                    existingColumn.Order = column.Order;
                    existingColumn.UpdatedAt = DateTime.Now;
                    
                    await _notifier.NotifyColumnUpdatedAsync(board.Id, existingColumn, _authService.CurrentUser?.Id);
                }
                else
                {
                    column.BoardId = board.Id;
                    existingBoard.Columns.Add(column);
                    
                    await _notifier.NotifyColumnAddedAsync(board.Id, column, _authService.CurrentUser?.Id);
                }
            }

            var columnsToRemove = existingColumns.Where(c => !board.Columns.Any(bc => bc.Id == c.Id)).ToList();
            foreach (var column in columnsToRemove)
            {
                context.Columns.Remove(column);
                await _notifier.NotifyColumnDeletedAsync(board.Id, column.Id);
            }

            foreach (var item in board.Items)
            {
                var existingItem = existingItems.FirstOrDefault(i => i.Id == item.Id);
                if (existingItem != null)
                {
                    var wasCompleted = existingItem.IsCompleted;
                    
                    existingItem.Title = item.Title;
                    existingItem.Description = item.Description;
                    existingItem.IsCompleted = item.IsCompleted;
                    existingItem.ColumnId = item.ColumnId;
                    existingItem.Order = item.Order;
                    existingItem.Priority = item.Priority;
                    existingItem.CurrentStepIndex = item.CurrentStepIndex;
                    existingItem.StartDate = item.StartDate;
                    existingItem.EndDate = item.EndDate;
                    existingItem.DueDate = item.DueDate;
                    existingItem.AssignedUsers = item.AssignedUsers ?? new Dictionary<string, string>();
                    existingItem.UpdatedAt = DateTime.Now;

                    var existingSteps = existingItem.Steps?.ToList() ?? new List<TodoStep>();
                    if (item.Steps != null)
                    {
                        foreach (var step in item.Steps)
                        {
                            var existingStep = existingSteps.FirstOrDefault(s => s.Id == step.Id);
                            if (existingStep != null)
                            {
                                existingStep.Name = step.Name;
                                existingStep.IsCompleted = step.IsCompleted;
                            }
                            else
                            {
                                step.ItemId = item.Id;
                                existingItem.Steps ??= new List<TodoStep>();
                                existingItem.Steps.Add(step);
                            }
                        }

                        var stepsToRemove = existingSteps.Where(s => !item.Steps!.Any(is2 => is2.Id == s.Id)).ToList();
                        foreach (var step in stepsToRemove)
                        {
                            context.Steps.Remove(step);
                        }
                    }
                    
                    await _notifier.NotifyItemUpdatedAsync(board.Id, existingItem, _authService.CurrentUser?.Id);
                }
                else
                {
                    item.ColumnId = board.Columns.FirstOrDefault()?.Id ?? "";
                    item.TodoBoardId = board.Id;
                    existingBoard.Items.Add(item);
                    
                    await _notifier.NotifyItemAddedAsync(board.Id, item.ColumnId, item, _authService.CurrentUser?.Id);
                }
            }

            var itemsToRemove = existingItems.Where(i => !board.Items.Any(bi => bi.Id == i.Id)).ToList();
            foreach (var item in itemsToRemove)
            {
                context.Items.Remove(item);
                await _notifier.NotifyItemDeletedAsync(board.Id, item.Id);
            }

            await context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DeleteBoardAsync(string boardId)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var board = await context.Boards
                .Include(b => b.Columns)
                .Include(b => b.Items)
                    .ThenInclude(i => i.Steps)
                .FirstOrDefaultAsync(b => b.Id == boardId);
            
            if (board == null) return false;

            context.Boards.Remove(board);
            await context.SaveChangesAsync();
            
            await _notifier.NotifyBoardDeletedAsync(boardId);
            
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<TodoBoard?> GetOrCreateDefaultBoardAsync(string userId)
    {
        var boards = await GetUserBoardsAsync(userId);
        if (boards.Any())
        {
            return boards.First();
        }

        return await CreateBoardAsync(userId, "Mi Tablero");
    }

    public void ClearCache()
    {
    }

    public async Task<CalendarEvent?> AddEventAsync(string boardId, string title, string? description, DateTime eventDate)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            var board = await context.Boards.FindAsync(boardId);
            if (board == null) return null;

            var newEvent = new CalendarEvent
            {
                Id = Guid.NewGuid().ToString(),
                Title = title,
                Description = description,
                EventDate = eventDate,
                TodoBoardId = boardId,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            context.CalendarEvents.Add(newEvent);
            await context.SaveChangesAsync();
            
            await _logService.AddLogAsync(new LogItem
            {
                Action = DatabaseAction.Crear,
                Message = $"Crea evento '{title}' para el {eventDate:dd/MM/yyyy}",
                BoardId = boardId,
                User = _authService.CurrentUser?.Username ?? "Sistema"
            });
            
            return newEvent;
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> DeleteEventAsync(string eventId)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            var evt = await context.CalendarEvents.FindAsync(eventId);
            if (evt == null) return false;

            var eventTitle = evt.Title;
            var boardId = evt.TodoBoardId;
            var eventDate = evt.EventDate;

            context.CalendarEvents.Remove(evt);
            await context.SaveChangesAsync();
            
            await _logService.AddLogAsync(new LogItem
            {
                Action = DatabaseAction.Remover,
                Message = $"Elimina evento '{eventTitle}' del {eventDate:dd/MM/yyyy}",
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
