using TodoApp2OpenCode.Models;

namespace TodoApp2OpenCode.Services;

public interface ILogService
{
    Task AddLogAsync(LogItem log);
    Task<IEnumerable<LogItem>> GetLogsByBoardId(string boardId);
}
