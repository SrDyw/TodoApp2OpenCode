using Microsoft.EntityFrameworkCore;
using TodoApp2OpenCode.Models;

namespace TodoApp2OpenCode.Data;

public interface IFlowBoardDbContext : IAsyncDisposable
{
    DbSet<TestEntity> TestEntities { get; set; }
    DbSet<LogItem> LogItems { get; set; }
    DbSet<TodoBoard> Boards { get; set; }
    DbSet<TodoColumn> Columns { get; set; }
    DbSet<TodoItem> Items { get; set; }
    DbSet<TodoStep> Steps { get; set; }
    DbSet<User> Users { get; set; }
    DbSet<CalendarEvent> CalendarEvents { get; set; }
    DbSet<Notification> Notifications { get; set; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
