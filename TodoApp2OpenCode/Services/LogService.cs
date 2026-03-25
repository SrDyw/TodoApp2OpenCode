using Microsoft.EntityFrameworkCore;
using TodoApp2OpenCode.Data;
using TodoApp2OpenCode.Models;

namespace TodoApp2OpenCode.Services;

public class LogService : ILogService
{
    private readonly FlowBoardDbContext _context;

    public LogService(FlowBoardDbContext context)
    {
        _context = context;
    }

    public async Task AddLogAsync(LogItem log)
    {
        log.Id = 0;
        log.CreatedAt = DateTime.Now;
        _context.LogItems.Add(log);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<LogItem>> GetLogsByBoardId(string boardId)
    {
        return await Task.FromResult(_context.LogItems.Where(x => x.BoardId == boardId).OrderByDescending(x => x.CreatedAt));
    }
}
