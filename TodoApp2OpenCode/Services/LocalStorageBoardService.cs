using System.Text.Json;
using Microsoft.JSInterop;
using TodoApp2OpenCode.Models;

namespace TodoApp2OpenCode.Services;

public class LocalStorageBoardService : IBoardService
{
    private readonly IJSRuntime _jsRuntime;
    private const string BOARDS_KEY = "flowboard_boards";

    private readonly JsonSerializerOptions _jsonOptions;
    private List<TodoBoard> _cachedBoards = new();
    private bool _isLoaded = false;

    public LocalStorageBoardService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };
    }

    public async Task<List<TodoBoard>> GetUserBoardsAsync(string userId)
    {
        if (!_isLoaded)
        {
            await LoadBoardsAsync();
        }
        return _cachedBoards
            .Where(b => b.User == userId || b.Participants.ContainsKey(userId))
            .OrderBy(b => b.Name)
            .ToList();
    }

    public async Task<TodoBoard?> GetBoardAsync(string boardId)
    {
        if (!_isLoaded)
        {
            await LoadBoardsAsync();
        }
        return _cachedBoards.FirstOrDefault(b => b.Id == boardId);
    }

    public async Task<TodoBoard?> CreateBoardAsync(string userId, string name, string? description = null, List<(string Id, string Name)>? participants = null)
    {
        try
        {
            if (!_isLoaded)
            {
                await LoadBoardsAsync();
            }

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
                Items = new List<TodoItem>(),
                Events = new List<CalendarEvent>()
            };

            _cachedBoards.Add(board);
            await SaveBoardsAsync();
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
            if (!_isLoaded)
            {
                await LoadBoardsAsync();
            }

            var board = _cachedBoards.FirstOrDefault(b => b.Id == boardId);
            if (board == null) return false;

            if (!board.Participants.ContainsKey(userId))
            {
                board.Participants[userId] = userName;
                await SaveBoardsAsync();
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
            if (!_isLoaded)
            {
                await LoadBoardsAsync();
            }

            var board = _cachedBoards.FirstOrDefault(b => b.Id == boardId);
            if (board == null) return false;

            board.Participants.Remove(userId);

            await SaveBoardsAsync();
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
            if (!_isLoaded)
            {
                await LoadBoardsAsync();
            }

            var index = _cachedBoards.FindIndex(b => b.Id == board.Id);
            if (index == -1) return false;

            _cachedBoards[index] = board;
            await SaveBoardsAsync();
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
            if (!_isLoaded)
            {
                await LoadBoardsAsync();
            }

            _cachedBoards.RemoveAll(b => b.Id == boardId);
            await SaveBoardsAsync();
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

    private async Task LoadBoardsAsync()
    {
        try
        {
            var json = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", BOARDS_KEY);
            if (!string.IsNullOrEmpty(json))
            {
                _cachedBoards = JsonSerializer.Deserialize<List<TodoBoard>>(json, _jsonOptions) ?? new List<TodoBoard>();
            }
        }
        catch
        {
            _cachedBoards = new List<TodoBoard>();
        }
        _isLoaded = true;
    }

    private async Task SaveBoardsAsync()
    {
        var json = JsonSerializer.Serialize(_cachedBoards, _jsonOptions);
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", BOARDS_KEY, json);
    }

    public void ClearCache()
    {
        _cachedBoards = new List<TodoBoard>();
        _isLoaded = false;
    }

    public Task<CalendarEvent?> AddEventAsync(string boardId, string title, string? description, DateTime eventDate, Dictionary<string, string>? participants = null)
    {
        try
        {
            var board = _cachedBoards.FirstOrDefault(b => b.Id == boardId);
            if (board == null) return Task.FromResult<CalendarEvent?>(null);

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

            board.Events ??= new List<CalendarEvent>();
            board.Events.Add(newEvent);
            _ = SaveBoardsAsync();
            return Task.FromResult<CalendarEvent?>(newEvent);
        }
        catch
        {
            return Task.FromResult<CalendarEvent?>(null);
        }
    }

    public Task<bool> DeleteEventAsync(string eventId)
    {
        try
        {
            foreach (var board in _cachedBoards)
            {
                var evt = board.Events?.FirstOrDefault(e => e.Id == eventId);
                if (evt != null)
                {
                    board.Events?.Remove(evt);
                    _ = SaveBoardsAsync();
                    return Task.FromResult(true);
                }
            }
            return Task.FromResult(false);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public Task<bool> UpdateEventAsync(string eventId, string title, string? description, DateTime eventDate, Dictionary<string, string>? participants = null)
    {
        try
        {
            foreach (var board in _cachedBoards)
            {
                var evt = board.Events?.FirstOrDefault(e => e.Id == eventId);
                if (evt != null)
                {
                    evt.Title = title;
                    evt.Description = description;
                    evt.EventDate = eventDate;
                    evt.Participants = participants ?? new Dictionary<string, string>();
                    evt.UpdatedAt = DateTime.Now;
                    _ = SaveBoardsAsync();
                    return Task.FromResult(true);
                }
            }
            return Task.FromResult(false);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public Task<List<CalendarEvent>> GetUserEventsAsync(string userId)
    {
        try
        {
            var allEvents = _cachedBoards
                .SelectMany(b => b.Events ?? new List<CalendarEvent>())
                .OrderBy(e => e.EventDate)
                .ToList();

            var userEvents = allEvents
                .Where(e => e.Participants?.ContainsKey(userId) ?? false)
                .ToList();
            return Task.FromResult(userEvents);
        }
        catch
        {
            return Task.FromResult(new List<CalendarEvent>());
        }
    }

    public Task<List<TodoItem>> GetUserItemsAsync(string userId)
    {
        try
        {
            var allItems = _cachedBoards
                .SelectMany(b => b.Items ?? new List<TodoItem>())
                .OrderBy(i => i.DueDate)
                .ToList();

            var userItems = allItems
                .Where(i => i.AssignedUsers?.ContainsKey(userId) ?? false)
                .ToList();
            return Task.FromResult(userItems);
        }
        catch
        {
            return Task.FromResult(new List<TodoItem>());
        }
    }
}
