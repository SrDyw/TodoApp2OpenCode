using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using TodoApp2OpenCode.Data;
using TodoApp2OpenCode.Models;

namespace TodoApp2OpenCode.Services;

public class AuthService : IAuthService
{
    private readonly IFlowBoardDbContextFactory _contextFactory;
    private readonly IJSRuntime _jsRuntime;
    private const string LAST_BOARD_KEY = "flowboard_last_board";
    private const string CURRENT_USER_KEY = "flowboard_current_user";
    private const string SALT = "FlowBoard_Secure_Salt_2024";

    private User? _currentUser;
    private bool _isInitialized = false;

    private Action<User?>? _onAuthStateChangedAction;

    public AuthService(IFlowBoardDbContextFactory contextFactory, IJSRuntime jsRuntime)
    {
        _contextFactory = contextFactory;
        _jsRuntime = jsRuntime;
    }

    public User? CurrentUser => _currentUser;
    public bool IsAuthenticated => _currentUser != null;

    public void SetAuthStateChangedCallback(Action<User?> callback)
    {
        _onAuthStateChangedAction = callback;
    }

    public void SetLastVisitedBoard(string boardId)
    {
        _ = SetLastVisitedBoardAsync(boardId);
    }

    private async Task SetLastVisitedBoardAsync(string boardId)
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", LAST_BOARD_KEY, boardId);
    }

    public string? GetLastVisitedBoard()
    {
        try
        {
            return _jsRuntime.InvokeAsync<string?>("localStorage.getItem", LAST_BOARD_KEY).Result;
        }
        catch
        {
            return null;
        }
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var saltedPassword = password + SALT;
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
        return Convert.ToBase64String(bytes);
    }

    public async Task<(bool Success, string? Error)> RegisterAsync(string username, string email, string password)
    {
        if (string.IsNullOrWhiteSpace(username))
            return (false, "El nombre de usuario es requerido");

        if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
            return (false, "Email inválido");

        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            return (false, "La contraseña debe tener al menos 6 caracteres");

        await using var context = await _contextFactory.CreateDbContextAsync();
        var users = await context.Users.ToListAsync();
        var emailExists = users.Any(u => u.Email.ToLower() == email.Trim().ToLower());
        if (emailExists)
            return (false, "El email ya está registrado");

        var usernameExists = users.Any(u => u.Username.ToLower() == username.Trim().ToLower());
        if (usernameExists)
            return (false, "El nombre de usuario ya está en uso");

        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Username = username.Trim(),
            Email = email.Trim().ToLower(),
            PasswordHash = HashPassword(password),
            CreatedAt = DateTime.Now
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        _currentUser = user;
        await SaveCurrentUserAsync(user);
        _onAuthStateChangedAction?.Invoke(_currentUser);

        return (true, null);
    }

    public async Task<(bool Success, string? Error)> LoginAsync(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email))
            return (false, "El email es requerido");

        if (string.IsNullOrWhiteSpace(password))
            return (false, "La contraseña es requerida");

        await using var context = await _contextFactory.CreateDbContextAsync();

        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.Trim().ToLower());

        if (user == null)
            return (false, "No existe una cuenta con este email");

        var passwordHash = HashPassword(password);
        if (user.PasswordHash != passwordHash)
            return (false, "Contraseña incorrecta");

        _currentUser = user;
        await SaveCurrentUserAsync(user);
        _onAuthStateChangedAction?.Invoke(_currentUser);

        return (true, null);
    }

    public async Task LogoutAsync()
    {
        _currentUser = null;
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", CURRENT_USER_KEY);
        _onAuthStateChangedAction?.Invoke(null);
    }

    public async Task<bool> CheckAuthStateAsync()
    {
        if (_isInitialized && _currentUser != null)
            return true;

        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            var userId = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", CURRENT_USER_KEY + "_id");
            if (!string.IsNullOrEmpty(userId))
            {
                _currentUser = await context.Users.FindAsync(userId);
                _isInitialized = true;
                return _currentUser != null;
            }
        }
        catch { }

        _isInitialized = true;
        return false;
    }

    private async Task SaveCurrentUserAsync(User user)
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", CURRENT_USER_KEY + "_id", user.Id);
    }

    public async Task<List<UserInfo>> SearchUsersAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm) || searchTerm.Length < 2)
            return new List<UserInfo>();

        await using var context = await _contextFactory.CreateDbContextAsync();

        var term = searchTerm.ToLower();

        return await context.Users
            .Where(u => u.Username.ToLower().Contains(term) || u.Email.ToLower().Contains(term))
            .Where(u => _currentUser == null || u.Id != _currentUser.Id)
            .Take(10)
            .Select(u => new UserInfo { Id = u.Id, Username = u.Username, Email = u.Email })
            .ToListAsync();
    }
}
