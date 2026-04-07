using TodoApp2OpenCode.Models;

namespace TodoApp2OpenCode.Services;

public interface IBoardService
{
    Task<List<TodoBoard>> GetUserBoardsAsync(string userId);
    Task<TodoBoard?> GetBoardAsync(string boardId);
    Task<TodoBoard?> CreateBoardAsync(string userId, string name, string? description = null, List<(string Id, string Name)>? participants = null);
    Task<bool> AddParticipantAsync(string boardId, string userId, string userName);
    Task<bool> RemoveParticipantAsync(string boardId, string userId);
    Task<bool> UpdateBoardAsync(TodoBoard board);
    Task<bool> DeleteBoardAsync(string boardId);
    Task<TodoBoard?> GetOrCreateDefaultBoardAsync(string userId);
    void ClearCache();
    Task<CalendarEvent?> AddEventAsync(string boardId, string title, string? description, DateTime eventDate, Dictionary<string, string>? participants = null);
    Task<bool> UpdateEventAsync(string eventId, string title, string? description, DateTime eventDate, Dictionary<string, string>? participants = null);
    Task<bool> DeleteEventAsync(string eventId);
}
