using Microsoft.EntityFrameworkCore;
using TodoApp2OpenCode.Data;
using TodoApp2OpenCode.Models;

namespace TodoApp2OpenCode.Services;

public class BoardService : IBoardService
{
    private readonly IDbContextFactory<FlowBoardDbContext> _contextFactory;

    public BoardService(IDbContextFactory<FlowBoardDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<TodoBoard>> GetUserBoardsAsync(string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var allBoards = await context.Boards.AsNoTracking().ToListAsync();
        return allBoards
            .Where(b => b.User == userId || b.ParticipantIds.Contains(userId))
            .OrderBy(b => b.Name)
            .ToList();
    }

    public async Task<TodoBoard?> GetBoardAsync(string boardId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var board = await context.Boards
            .AsNoTracking()
            .Include(b => b.Columns)
            .FirstOrDefaultAsync(b => b.Id == boardId);

        if (board != null)
        {
            var items = await context.Items
                .AsNoTracking()
                .Include(i => i.Steps)
                .Where(i => i.TodoBoardId == boardId)
                .ToListAsync();
            
            board.Items = items;
        }

        return board;
    }

    public async Task<TodoBoard?> CreateBoardAsync(string userId, string name, string? description = null, List<(string Id, string Name)>? participants = null)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            
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

            context.Boards.Add(board);
            await context.SaveChangesAsync();
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
            await using var context = await _contextFactory.CreateDbContextAsync();
            var board = await context.Boards.FindAsync(boardId);
            if (board == null) return false;

            if (!board.ParticipantIds.Contains(userId))
            {
                board.ParticipantIds.Add(userId);
                board.ParticipantNames[userId] = userName;
                await context.SaveChangesAsync();
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
            await using var context = await _contextFactory.CreateDbContextAsync();
            var board = await context.Boards.FindAsync(boardId);
            if (board == null) return false;

            board.ParticipantIds.Remove(userId);
            board.ParticipantNames.Remove(userId);

            await context.SaveChangesAsync();
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
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            var existingBoard = await context.Boards
                .Include(b => b.Columns)
                .Include(b => b.Items)
                    .ThenInclude(i => i.Steps)
                .FirstOrDefaultAsync(b => b.Id == board.Id);

            if (existingBoard == null) return false;

            existingBoard.Name = board.Name;
            existingBoard.Description = board.Description;
            existingBoard.ParticipantIds = board.ParticipantIds;
            existingBoard.ParticipantNames = board.ParticipantNames;
            existingBoard.OwnerName = board.OwnerName;

            var existingColumns = existingBoard.Columns.ToList();
            var existingItems = existingBoard.Items.ToList();

            foreach (var column in board.Columns)
            {
                var existingColumn = existingColumns.FirstOrDefault(c => c.Id == column.Id);
                if (existingColumn != null)
                {
                    existingColumn.Name = column.Name;
                    existingColumn.Color = column.Color;
                    existingColumn.UpdatedAt = DateTime.Now;
                }
                else
                {
                    column.BoardId = board.Id;
                    existingBoard.Columns.Add(column);
                }
            }

            var columnsToRemove = existingColumns.Where(c => !board.Columns.Any(bc => bc.Id == c.Id)).ToList();
            foreach (var column in columnsToRemove)
            {
                context.Columns.Remove(column);
            }

            foreach (var item in board.Items)
            {
                var existingItem = existingItems.FirstOrDefault(i => i.Id == item.Id);
                if (existingItem != null)
                {
                    existingItem.Title = item.Title;
                    existingItem.Description = item.Description;
                    existingItem.IsCompleted = item.IsCompleted;
                    existingItem.ColumnId = item.ColumnId;
                    existingItem.Order = item.Order;
                    existingItem.Priority = item.Priority;
                    existingItem.CurrentStepIndex = item.CurrentStepIndex;
                    existingItem.UpdatedAt = DateTime.Now;

                    var existingSteps = existingItem.Steps?.ToList() ?? new List<TodoStep>();
                    if (item.Steps != null)
                    {
                        foreach (var step in item.Steps)
                        {
                            var existingStep = existingSteps.FirstOrDefault(s => s.Id == step.Id);
                            if (existingStep != null)
                            {
                                existingStep.Name = step.Name;
                                existingStep.IsCompleted = step.IsCompleted;
                            }
                            else
                            {
                                step.ItemId = item.Id;
                                existingItem.Steps ??= new List<TodoStep>();
                                existingItem.Steps.Add(step);
                            }
                        }

                        var stepsToRemove = existingSteps.Where(s => !item.Steps!.Any(is2 => is2.Id == s.Id)).ToList();
                        foreach (var step in stepsToRemove)
                        {
                            context.Steps.Remove(step);
                        }
                    }
                }
                else
                {
                    item.ColumnId = board.Columns.FirstOrDefault()?.Id ?? "";
                    existingBoard.Items.Add(item);
                }
            }

            var itemsToRemove = existingItems.Where(i => !board.Items.Any(bi => bi.Id == i.Id)).ToList();
            foreach (var item in itemsToRemove)
            {
                context.Items.Remove(item);
            }

            await context.SaveChangesAsync();
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
            await using var context = await _contextFactory.CreateDbContextAsync();
            var board = await context.Boards.FindAsync(boardId);
            if (board == null) return false;

            context.Boards.Remove(board);
            await context.SaveChangesAsync();
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

    public void ClearCache()
    {
    }
}
