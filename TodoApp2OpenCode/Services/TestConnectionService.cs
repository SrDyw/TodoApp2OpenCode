using Microsoft.EntityFrameworkCore;
using TodoApp2OpenCode.Data;
using TodoApp2OpenCode.Models;

namespace TodoApp2OpenCode.Services;

public class TestConnectionService
{
    private readonly IFlowBoardDbContextFactory _contextFactory;

    public TestConnectionService(IFlowBoardDbContextFactory contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<TestEntity>> GetAllAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.TestEntities.ToListAsync();
    }
}
