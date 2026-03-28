using Microsoft.EntityFrameworkCore;

namespace TodoApp2OpenCode.Data;

public class FlowBoardDbContextFactory<TContext> : IFlowBoardDbContextFactory 
    where TContext : DbContext, IFlowBoardDbContext
{
    private readonly IDbContextFactory<TContext> _contextFactory;

    public FlowBoardDbContextFactory(IDbContextFactory<TContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<IFlowBoardDbContext> CreateDbContextAsync()
    {
        return await _contextFactory.CreateDbContextAsync();
    }
}
