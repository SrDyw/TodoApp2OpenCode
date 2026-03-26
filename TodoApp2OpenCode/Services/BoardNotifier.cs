using Microsoft.AspNetCore.SignalR;
using System.Text.Json;
using TodoApp2OpenCode.Hubs;
using TodoApp2OpenCode.Models;

namespace TodoApp2OpenCode.Services;

public interface IBoardNotifier
{
    Task NotifyBoardUpdatedAsync(string boardId, string? excludeUserId);
    Task NotifyBoardDeletedAsync(string boardId);
    Task NotifyColumnAddedAsync(string boardId, TodoColumn column, string? excludeUserId);
    Task NotifyColumnUpdatedAsync(string boardId, TodoColumn column, string? excludeUserId);
    Task NotifyColumnDeletedAsync(string boardId, string columnId);
    Task NotifyItemAddedAsync(string boardId, string columnId, TodoItem item, string? excludeUserId);
    Task NotifyItemUpdatedAsync(string boardId, TodoItem item, string? excludeUserId);
    Task NotifyItemDeletedAsync(string boardId, string itemId);
}

public class BoardNotifier : IBoardNotifier
{
    private readonly IHubContext<BoardHub, IBoardHubClient> _hubContext;
    private readonly Dictionary<string, List<DateTime>> _notificationHistory = new();
    private System.Timers.Timer? _cleanupTimer;
    private readonly object _lock = new();
    private const int DebounceMs = 150;
    private const int MaxHistoryEntries = 100;

    public BoardNotifier(IHubContext<BoardHub, IBoardHubClient> hubContext)
    {
        _hubContext = hubContext;
        
        _cleanupTimer = new System.Timers.Timer(30000);
        _cleanupTimer.Elapsed += (s, e) => CleanupHistory();
        _cleanupTimer.Start();
    }

    private bool ShouldSend(string key)
    {
        var now = DateTime.UtcNow;
        
        lock (_lock)
        {
            if (!_notificationHistory.ContainsKey(key))
            {
                _notificationHistory[key] = new List<DateTime>();
            }

            var recentNotifications = _notificationHistory[key]
                .Where(t => (now - t).TotalMilliseconds < DebounceMs)
                .ToList();

            _notificationHistory[key] = recentNotifications;

            if (recentNotifications.Count > 0)
            {
                return false;
            }

            _notificationHistory[key].Add(now);
            return true;
        }
    }

    private void CleanupHistory()
    {
        lock (_lock)
        {
            var cutoff = DateTime.UtcNow.AddMinutes(-1);
            foreach (var key in _notificationHistory.Keys.ToList())
            {
                _notificationHistory[key] = _notificationHistory[key]
                    .Where(t => t > cutoff)
                    .ToList();

                if (_notificationHistory[key].Count == 0)
                {
                    _notificationHistory.Remove(key);
                }
            }

            while (_notificationHistory.Values.Sum(v => v.Count) > MaxHistoryEntries)
            {
                var oldest = _notificationHistory.Values
                    .SelectMany(v => v)
                    .OrderBy(t => t)
                    .FirstOrDefault();
                
                if (oldest != default)
                {
                    foreach (var key in _notificationHistory.Keys.ToList())
                    {
                        _notificationHistory[key].Remove(oldest);
                        if (_notificationHistory[key].Count == 0)
                        {
                            _notificationHistory.Remove(key);
                            break;
                        }
                    }
                }
            }
        }
    }

    public async Task NotifyBoardUpdatedAsync(string boardId, string? excludeUserId)
    {
        var key = $"board_updated_{boardId}";
        if (!ShouldSend(key)) return;

        await _hubContext.Clients.Group(boardId).BoardUpdated(boardId, excludeUserId);
    }

    public async Task NotifyBoardDeletedAsync(string boardId)
    {
        await _hubContext.Clients.Group(boardId).BoardDeleted(boardId);
    }

    public async Task NotifyColumnAddedAsync(string boardId, TodoColumn column, string? excludeUserId)
    {
        var key = $"column_added_{boardId}_{column.Id}";
        if (!ShouldSend(key)) return;

        var json = JsonSerializer.Serialize(column, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        await _hubContext.Clients.Group(boardId).ColumnAdded(boardId, json, excludeUserId);
    }

    public async Task NotifyColumnUpdatedAsync(string boardId, TodoColumn column, string? excludeUserId)
    {
        var key = $"column_updated_{boardId}_{column.Id}";
        if (!ShouldSend(key)) return;

        var json = JsonSerializer.Serialize(column, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        await _hubContext.Clients.Group(boardId).ColumnUpdated(boardId, json, excludeUserId);
    }

    public async Task NotifyColumnDeletedAsync(string boardId, string columnId)
    {
        await _hubContext.Clients.Group(boardId).ColumnDeleted(boardId, columnId);
    }

    public async Task NotifyItemAddedAsync(string boardId, string columnId, TodoItem item, string? excludeUserId)
    {
        var key = $"item_added_{boardId}_{columnId}_{item.Id}";
        if (!ShouldSend(key)) return;

        var json = JsonSerializer.Serialize(item, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        await _hubContext.Clients.Group(boardId).ItemAdded(boardId, columnId, json, excludeUserId);
    }

    public async Task NotifyItemUpdatedAsync(string boardId, TodoItem item, string? excludeUserId)
    {
        var key = $"item_updated_{boardId}_{item.Id}";
        if (!ShouldSend(key)) return;

        var json = JsonSerializer.Serialize(item, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        await _hubContext.Clients.Group(boardId).ItemUpdated(boardId, json, excludeUserId);
    }

    public async Task NotifyItemDeletedAsync(string boardId, string itemId)
    {
        await _hubContext.Clients.Group(boardId).ItemDeleted(boardId, itemId);
    }
}
