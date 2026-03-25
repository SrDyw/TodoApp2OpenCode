using System.Text.Json;
using Microsoft.JSInterop;
using TodoApp2OpenCode.Models;

namespace TodoApp2OpenCode.Services;

public class LocalStorageLogService : ILogService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly JsonSerializerOptions _jsonOptions;
    private List<LogItem> _cache = [];
    private const string LOG_KEY = "logs";
    private bool _loaded = false;

    public LocalStorageLogService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };
    }

    public async Task SaveLogsAsync()
    {
        var json = JsonSerializer.Serialize(_cache, _jsonOptions);
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", LOG_KEY, json);
    }

    public async Task LoadLogsAsync()
    {
        if (_loaded) return;

        var json = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", LOG_KEY);
        var data = JsonSerializer.Deserialize<List<LogItem>>(json, _jsonOptions) ?? [];
        _cache = data;
        _loaded = true;
    }

    public async Task AddLogAsync(LogItem log)
    {
        _cache.Add(log);
        await SaveLogsAsync();
    }

    public async Task<IEnumerable<LogItem>> GetLogsByBoardId(string boardId)
    {
        if (!_loaded)
        {
            await LoadLogsAsync();
        }
        return _cache.Where(x => x.BoardId == boardId);
    }
}
