using Microsoft.EntityFrameworkCore;
using TodoApp2OpenCode.Data;
using TodoApp2OpenCode.Models;

namespace TodoApp2OpenCode.Services;

public class BoardService : IBoardService
{
    private readonly IFlowBoardDbContextFactory _contextFactory;
    private readonly IBoardNotifier _notifier;
    private readonly IAuthService _authService;
    private readonly ILogService _logService;
    private readonly INotificationService _notificationService;

    public BoardService(IFlowBoardDbContextFactory contextFactory, IBoardNotifier notifier, IAuthService authService, ILogService logService, INotificationService notificationService)
    {
        _contextFactory = contextFactory;
        _notifier = notifier;
        _authService = authService;
        _logService = logService;
        _notificationService = notificationService;
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
                if (item.Steps != null)
                {
                    item.Steps = item.Steps.OrderBy(s => s.Order).ToList();
                }
            }
            
            board.Items = items;

            if (!string.IsNullOrEmpty(board.ParticipantPermissionsJson))
            {
                board.ParticipantPermissions = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, BoardPermissions>>(board.ParticipantPermissionsJson) ?? new();
            }
            else
            {
                board.ParticipantPermissions = new Dictionary<string, BoardPermissions>();
            }
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

    public async Task<bool> AddParticipantAsync(string boardId, string userId, string userName, BoardPermissions? permissions = null)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var board = await context.Boards.FindAsync(boardId);
            if (board == null) return false;

            if (!board.Participants.ContainsKey(userId))
            {
                board.Participants[userId] = userName;
                
                var perms = permissions ?? new BoardPermissions
                {
                    CanViewCalendar = true,
                    CanAddTasks = true,
                    CanModifyTasks = true,
                    CanDeleteTasks = true,
                    CanAddEvents = false,
                    CanModifyEvents = false,
                    CanDeleteEvents = false
                };
                
                var existingPerms = board.ParticipantPermissions;
                existingPerms[userId] = perms;
                board.ParticipantPermissions = existingPerms;
                
                await context.SaveChangesAsync();
                
                await _notificationService.CreateAsync(
                    userId,
                    "Nuevo board",
                    $"Has sido añadido al board '{board.Name}'",
                    $"/board/{boardId}"
                );

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
            board.ParticipantPermissions.Remove(userId);

            await context.SaveChangesAsync();
            
            await _notifier.NotifyBoardUpdatedAsync(boardId, _authService.CurrentUser?.Id);
            
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> UpdateParticipantPermissionsAsync(string boardId, string userId, BoardPermissions permissions)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var board = await context.Boards.FindAsync(boardId);
            if (board == null) return false;

            var permsDict = board.ParticipantPermissions;
            permsDict[userId] = permissions;
            board.ParticipantPermissions = permsDict;
            
            await context.SaveChangesAsync();
            
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool HasPermission(string boardId, string userId, string permission)
    {
        var board = GetBoardAsync(boardId).GetAwaiter().GetResult();
        if (board == null) return false;

        var currentUserId = _authService.CurrentUser?.Id ?? "";
        
        if (board.User == currentUserId)
        {
            return true;
        }

        if (board.ParticipantPermissions.TryGetValue(userId, out var perms))
        {
            return permission switch
            {
                "CanAddTasks" => perms.CanAddTasks,
                "CanModifyTasks" => perms.CanModifyTasks,
                "CanDeleteTasks" => perms.CanDeleteTasks,
                "CanAddEvents" => perms.CanAddEvents,
                "CanModifyEvents" => perms.CanModifyEvents,
                "CanDeleteEvents" => perms.CanDeleteEvents,
                _ => false
            };
        }

        return false;
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
                        int maxOrder = existingSteps.Any() ? existingSteps.Max(s => s.Order) : -1;
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
                                step.Order = ++maxOrder;
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
                    
                    if (item.Steps != null)
                    {
                        int order = 0;
                        foreach (var step in item.Steps)
                        {
                            step.ItemId = item.Id;
                            step.Order = order++;
                        }
                    }
                    
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

    public async Task<CalendarEvent?> AddEventAsync(string boardId, string title, string? description, DateTime eventDate, Dictionary<string, string>? participants = null)
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
                Participants = participants ?? new Dictionary<string, string>(),
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

            if (participants != null)
            {
                foreach (var participant in participants)
                {
                    await _notificationService.CreateAsync(
                        participant.Key,
                        "Nuevo evento",
                        $"Has sido añadido al evento '{title}' el {eventDate:dd/MM/yyyy}",
                        $"/board/{boardId}/schedule?date={eventDate:dd-MM-yyyy}"
                    );
                }
            }
            
            return newEvent;
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> UpdateEventAsync(string eventId, string title, string? description, DateTime eventDate, Dictionary<string, string>? participants = null)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            var evt = await context.CalendarEvents.FindAsync(eventId);
            if (evt == null) return false;

            var oldTitle = evt.Title;
            var oldDate = evt.EventDate;
            var oldParticipants = new Dictionary<string, string>(evt.Participants);

            evt.Title = title;
            evt.Description = description;
            evt.EventDate = eventDate;
            evt.Participants = participants ?? new Dictionary<string, string>();
            evt.UpdatedAt = DateTime.Now;

            await context.SaveChangesAsync();
            
            var boardId = evt.TodoBoardId;
            var logMessage = $"Actualiza evento '{title}'";
            if (oldDate.Date != eventDate.Date)
            {
                logMessage += $" (fecha cambiada de {oldDate:dd/MM/yyyy} a {eventDate:dd/MM/yyyy})";
            }
            
            await _logService.AddLogAsync(new LogItem
            {
                Action = DatabaseAction.Actualizar,
                Message = logMessage,
                BoardId = boardId,
                User = _authService.CurrentUser?.Username ?? "Sistema"
            });

            var participantKeys = participants?.Keys ?? new Dictionary<string, string>().Keys;
            var addedParticipants = participantKeys.Except(oldParticipants.Keys).ToList();
            var removedParticipants = oldParticipants.Keys.Except(participantKeys).ToList();

            if (oldTitle != title)
            {
                foreach (var oldParticipantId in oldParticipants.Keys)
                {
                    await _notificationService.CreateAsync(
                        oldParticipantId,
                        "Evento actualizado",
                        $"El evento '{oldTitle}' ha sido renombrado a '{title}' el {eventDate:dd/MM/yyyy}",
                        $"/board/{boardId}/schedule?date={eventDate:dd-MM-yyyy}"
                    );
                }
            }

            foreach (var participantId in addedParticipants)
            {
                if (participants != null && participants.TryGetValue(participantId, out var name))
                {
                    await _notificationService.CreateAsync(
                        participantId,
                        "Nuevo evento",
                        $"Has sido añadido al evento '{title}' el {eventDate:dd/MM/yyyy}",
                        $"/board/{boardId}/schedule?date={eventDate:dd-MM-yyyy}"
                    );
                }
            }

            foreach (var participantId in removedParticipants)
            {
                if (oldParticipants.TryGetValue(participantId, out var name))
                {
                    await _notificationService.CreateAsync(
                        participantId,
                        "Evento eliminado",
                        $"Has sido eliminado del evento '{title}'",
                        $"/board/{boardId}/schedule"
                    );
                }
            }

            return true;
        }
        catch
        {
            return false;
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

    public async Task<List<CalendarEvent>> GetUserEventsAsync(string userId)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            var allEvents = await context.CalendarEvents
                .OrderBy(e => e.EventDate)
                .ToListAsync();

            var userEvents = new List<CalendarEvent>();
            foreach (var evt in allEvents)
            {
                if (!string.IsNullOrEmpty(evt.ParticipantsJson))
                {
                    evt.Participants = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(evt.ParticipantsJson) ?? new();
                }
                
                if (evt.Participants.ContainsKey(userId))
                {
                    userEvents.Add(evt);
                }
            }

            return userEvents;
        }
        catch
        {
            return new List<CalendarEvent>();
        }
    }

    public async Task<List<TodoItem>> GetUserItemsAsync(string userId)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            var allItems = await context.Items
                .OrderBy(i => i.DueDate)
                .ToListAsync();

            var userItems = new List<TodoItem>();
            foreach (var item in allItems)
            {
                if (!string.IsNullOrEmpty(item.AssignedUsersJson))
                {
                    item.AssignedUsers = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(item.AssignedUsersJson) ?? new();
                }
                
                if (item.AssignedUsers.ContainsKey(userId))
                {
                    userItems.Add(item);
                }
            }

            return userItems;
        }
        catch
        {
            return new List<TodoItem>();
        }
    }
}
