using Microsoft.EntityFrameworkCore;
using TodoApp2OpenCode.Data;
using TodoApp2OpenCode.Models;

namespace TodoApp2OpenCode.Services;

public class LogService : ILogService
{
    private readonly IFlowBoardDbContextFactory _contextFactory;

    public LogService(IFlowBoardDbContextFactory contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task AddLogAsync(LogItem log)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        log.Id = 0;
        log.CreatedAt = DateTime.Now;
        context.LogItems.Add(log);
        await context.SaveChangesAsync();
    }

    public async Task<IEnumerable<LogItem>> GetLogsByBoardId(string boardId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var logs = await context.LogItems
            .Where(x => x.BoardId == boardId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
        return logs;
    }
}
