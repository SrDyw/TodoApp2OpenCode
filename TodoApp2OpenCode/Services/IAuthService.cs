using TodoApp2OpenCode.Models;

namespace TodoApp2OpenCode.Services;

public interface IAuthService
{
    User? CurrentUser { get; }
    bool IsAuthenticated { get; }
    Task<(bool Success, string? Error)> RegisterAsync(string username, string email, string password);
    Task<(bool Success, string? Error)> LoginAsync(string email, string password);
    Task LogoutAsync();
    Task<bool> CheckAuthStateAsync();
    Task<List<UserInfo>> SearchUsersAsync(string searchTerm);
    void SetLastVisitedBoard(string boardId);
    string? GetLastVisitedBoard();
    void SetAuthStateChangedCallback(Action<User?> callback);
}

public class UserInfo
{
    public string Id { get; set; } = "";
    public string Username { get; set; } = "";
    public string Email { get; set; } = "";
}
