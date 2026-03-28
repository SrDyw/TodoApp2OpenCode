namespace TodoApp2OpenCode.Data;

public interface IFlowBoardDbContextFactory
{
    Task<IFlowBoardDbContext> CreateDbContextAsync();
}
