namespace TodoApp2OpenCode.Services;

public class BottomNavService
{
    public event Func<string, Task>? OnBottomNavAction;
    
    public async Task NotifyBottomNavAction(string action)
    {
        if (OnBottomNavAction != null)
        {
            await OnBottomNavAction.Invoke(action);
        }
    }
}
