using Microsoft.EntityFrameworkCore;
using TodoApp2OpenCode.Data;
using TodoApp2OpenCode.Models;

namespace TodoApp2OpenCode.Services;

public class TestConnectionService
{
    private readonly FlowBoardDbContext _context;

    public TestConnectionService(FlowBoardDbContext context)
    {
        _context = context;
    }

    public async Task<List<TestEntity>> GetAllAsync()
    {
        return await _context.TestEntities.ToListAsync();
    }
}
