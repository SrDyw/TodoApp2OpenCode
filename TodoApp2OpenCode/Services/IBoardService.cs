using TodoApp2OpenCode.Models;

namespace TodoApp2OpenCode.Services;

public interface IBoardService
{
    Task<List<TodoBoard>> GetUserBoardsAsync(string userId);
    Task<(string, TodoBoard?)> GetBoardAsync(string boardId);
    Task<(string, TodoBoard?)> CreateBoardAsync(string userId, string name, string? description = null, List<(string Id, string Name)>? participants = null);
    Task<(string, bool)> AddParticipantAsync(string boardId, string userId, string userName, BoardPermissions? permissions = null);
    Task<(string, bool)> RemoveParticipantAsync(string boardId, string userId);
    Task<TodoBoard?> UpdateBoardAsync(TodoBoard board);
    Task<(string, bool)> UpdateParticipantPermissionsAsync(string boardId, string userId, BoardPermissions permissions);
    Task<(string, bool)> DeleteBoardAsync(string boardId);
    Task<(string, bool)> SwapColumnAsync(string boardId, string columntoSwap, string targetColumnId);
    Task<(string, CalendarEvent?)> AddEventAsync(string boardId, string title, string? description, DateTime eventDate, Dictionary<string, string>? participants = null);
    Task<(string, bool)> UpdateEventAsync(string boardId, string eventId, string title, string? description, DateTime eventDate, Dictionary<string, string>? participants = null);
    Task<(string, bool)> DeleteEventAsync(string boardId, string eventId);
    Task<List<CalendarEvent>> GetUserEventsAsync(string userId);
    Task<List<TodoItem>> GetUserItemsAsync(string userId);

    Task<(string, bool)> AddColumnAsync(string boardId, TodoColumn column);
    Task<(string, bool)> UpdateColumnAsync(string boardId, TodoColumn column);
    Task<(string, bool)> DeleteColumnAsync(string boardId, string columnId);
    Task<string?> GetBoardNameAsync(string boardId);
}