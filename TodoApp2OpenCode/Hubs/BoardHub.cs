using Microsoft.AspNetCore.SignalR;

namespace TodoApp2OpenCode.Hubs;

public class BoardHub : Hub<IBoardHubClient>
{
    public async Task JoinBoard(string boardId)
    {
        System.Diagnostics.Debug.WriteLine($"BoardHub: Connection {Context.ConnectionId} joining board {boardId}");
        await Groups.AddToGroupAsync(Context.ConnectionId, boardId);
        System.Diagnostics.Debug.WriteLine($"BoardHub: Connection {Context.ConnectionId} joined board {boardId}");
    }

    public async Task LeaveBoard(string boardId)
    {
        System.Diagnostics.Debug.WriteLine($"BoardHub: Connection {Context.ConnectionId} leaving board {boardId}");
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, boardId);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        System.Diagnostics.Debug.WriteLine($"BoardHub: Connection {Context.ConnectionId} disconnected");
        await base.OnDisconnectedAsync(exception);
    }
}

public interface IBoardHubClient
{
    Task BoardUpdated(string boardId, string? excludeUserId);
    Task BoardDeleted(string boardId);
    Task ColumnAdded(string boardId, string columnJson, string? excludeUserId);
    Task ColumnUpdated(string boardId, string columnJson, string? excludeUserId);
    Task ColumnDeleted(string boardId, string columnId);
    Task ItemAdded(string boardId, string columnId, string itemJson, string? excludeUserId);
    Task ItemUpdated(string boardId, string itemJson, string? excludeUserId);
    Task ItemDeleted(string boardId, string itemId);
}
