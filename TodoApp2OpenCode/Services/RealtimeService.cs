using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using TodoApp2OpenCode.Models;

namespace TodoApp2OpenCode.Services;

public interface IRealtimeService
{
    event Action<string, string?>? OnBoardUpdated;
    event Action<string>? OnBoardDeleted;
    event Action<string, string, string?>? OnColumnAdded;
    event Action<string, string, string?>? OnColumnUpdated;
    event Action<string, string>? OnColumnDeleted;
    event Action<string, string, string, string?>? OnItemAdded;
    event Action<string, string, string?>? OnItemUpdated;
    event Action<string, string>? OnItemDeleted;
    
    void SetCurrentUserId(string? userId);
    Task ConnectAsync();
    Task DisconnectAsync();
    Task JoinBoardAsync(string boardId);
    Task LeaveBoardAsync(string boardId);
    bool IsConnected { get; }
}

public class RealtimeService : IRealtimeService, IAsyncDisposable
{
    private HubConnection? _hubConnection;
    private string? _currentBoardId;
    private string? _currentUserId;
    private readonly NavigationManager _navigationManager;

    public RealtimeService(NavigationManager navigationManager)
    {
        _navigationManager = navigationManager;
    }

    public event Action<string, string?>? OnBoardUpdated;
    public event Action<string>? OnBoardDeleted;
    public event Action<string, string, string?>? OnColumnAdded;
    public event Action<string, string, string?>? OnColumnUpdated;
    public event Action<string, string>? OnColumnDeleted;
    public event Action<string, string, string, string?>? OnItemAdded;
    public event Action<string, string, string?>? OnItemUpdated;
    public event Action<string, string>? OnItemDeleted;

    public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

    public void SetCurrentUserId(string? userId)
    {
        _currentUserId = userId;
    }

    public async Task ConnectAsync()
    {
        if (_hubConnection != null && _hubConnection.State != HubConnectionState.Disconnected)
            return;

        var baseUrl = _navigationManager.BaseUri.TrimEnd('/');
        var hubUrl = $"{baseUrl}/boardhub";
        
        _hubConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .WithAutomaticReconnect(new[] { TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10) })
            .Build();

        RegisterHandlers();

        try
        {
            await _hubConnection.StartAsync();
        }
        catch
        {
        }
    }

    private void RegisterHandlers()
    {
        if (_hubConnection == null) return;

        _hubConnection.On<string, string?>("BoardUpdated", (boardId, excludeUserId) =>
        {
            if (boardId == _currentBoardId && excludeUserId != _currentUserId)
                OnBoardUpdated?.Invoke(boardId, excludeUserId);
        });

        _hubConnection.On<string>("BoardDeleted", (boardId) =>
        {
            OnBoardDeleted?.Invoke(boardId);
        });

        _hubConnection.On<string, string, string?>("ColumnAdded", (boardId, columnJson, excludeUserId) =>
        {
            if (boardId == _currentBoardId && excludeUserId != _currentUserId)
                OnColumnAdded?.Invoke(boardId, columnJson, excludeUserId);
        });

        _hubConnection.On<string, string, string?>("ColumnUpdated", (boardId, columnJson, excludeUserId) =>
        {
            if (boardId == _currentBoardId && excludeUserId != _currentUserId)
                OnColumnUpdated?.Invoke(boardId, columnJson, excludeUserId);
        });

        _hubConnection.On<string, string>("ColumnDeleted", (boardId, columnId) =>
        {
            if (boardId == _currentBoardId)
                OnColumnDeleted?.Invoke(boardId, columnId);
        });

        _hubConnection.On<string, string, string, string?>("ItemAdded", (boardId, columnId, itemJson, excludeUserId) =>
        {
            if (boardId == _currentBoardId && excludeUserId != _currentUserId)
                OnItemAdded?.Invoke(boardId, columnId, itemJson, excludeUserId);
        });

        _hubConnection.On<string, string, string?>("ItemUpdated", (boardId, itemJson, excludeUserId) =>
        {
            if (boardId == _currentBoardId && excludeUserId != _currentUserId)
                OnItemUpdated?.Invoke(boardId, itemJson, excludeUserId);
        });

        _hubConnection.On<string, string>("ItemDeleted", (boardId, itemId) =>
        {
            if (boardId == _currentBoardId)
                OnItemDeleted?.Invoke(boardId, itemId);
        });

        _hubConnection.Reconnecting += (error) =>
        {
            return Task.CompletedTask;
        };

        _hubConnection.Reconnected += (connectionId) =>
        {
            if (_currentBoardId != null)
            {
                _ = JoinBoardAsync(_currentBoardId);
            }
            return Task.CompletedTask;
        };

        _hubConnection.Closed += (error) =>
        {
            return Task.CompletedTask;
        };
    }

    public async Task JoinBoardAsync(string boardId)
    {
        if (_hubConnection == null)
            await ConnectAsync();

        if (_hubConnection != null && _hubConnection.State == HubConnectionState.Connected)
        {
            if (_currentBoardId != null && _currentBoardId != boardId)
            {
                await _hubConnection.InvokeAsync("LeaveBoard", _currentBoardId);
            }

            _currentBoardId = boardId;
            await _hubConnection.InvokeAsync("JoinBoard", boardId);
        }
    }

    public async Task LeaveBoardAsync(string boardId)
    {
        if (_hubConnection != null)
        {
            await _hubConnection.InvokeAsync("LeaveBoard", boardId);
            if (_currentBoardId == boardId)
                _currentBoardId = null;
        }
    }

    public async Task DisconnectAsync()
    {
        if (_hubConnection != null)
        {
            if (_currentBoardId != null)
            {
                await _hubConnection.InvokeAsync("LeaveBoard", _currentBoardId);
            }
            await _hubConnection.StopAsync();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection != null)
        {
            await _hubConnection.DisposeAsync();
        }
    }
}
