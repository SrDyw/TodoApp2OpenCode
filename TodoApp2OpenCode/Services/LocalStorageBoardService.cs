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
            .Where(b => b.User == userId || b.ParticipantIds.Contains(userId))
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

            var participantIds = participants?.Select(p => p.Id).ToList() ?? new List<string>();
            var participantNames = participants?.ToDictionary(p => p.Id, p => p.Name) ?? new Dictionary<string, string>();

            var board = new TodoBoard
            {
                Id = Guid.NewGuid().ToString(),
                User = userId,
                Name = name,
                Description = description ?? string.Empty,
                ParticipantIds = participantIds,
                ParticipantNames = participantNames,
                OwnerName = string.Empty,
                Columns = new List<TodoColumn>(),
                Items = new List<TodoItem>()
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

            if (!board.ParticipantIds.Contains(userId))
            {
                board.ParticipantIds.Add(userId);
                board.ParticipantNames[userId] = userName;
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

            var user = board.ParticipantNames[userId];
        
            board.ParticipantIds.Remove(userId);
            board.ParticipantNames.Remove(userId);

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
}
