namespace TodoApp2OpenCode.Models;

public class BoardPermissions
{
    public bool CanAddTasks { get; set; } = true;
    public bool CanModifyTasks { get; set; } = true;
    public bool CanDeleteTasks { get; set; } = true;
    public bool CanAddEvents { get; set; } = true;
    public bool CanModifyEvents { get; set; } = true;
    public bool CanDeleteEvents { get; set; } = true;
}
