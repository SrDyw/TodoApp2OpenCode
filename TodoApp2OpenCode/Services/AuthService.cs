using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.JSInterop;
using TodoApp2OpenCode.Models;

namespace TodoApp2OpenCode.Services;

public class AuthService
{
    private readonly IJSRuntime _jsRuntime;
    private const string USERS_KEY = "flowboard_users";
    private const string CURRENT_USER_KEY = "flowboard_current_user";
    private const string LAST_BOARD_KEY = "flowboard_last_board";
    private const string SALT = "FlowBoard_Secure_Salt_2024";

    private UserWithPassword? _currentUser;
    private bool _isInitialized = false;

    private Action<UserWithPassword?>? _onAuthStateChangedAction;

    public AuthService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public UserWithPassword? CurrentUser => _currentUser;
    public bool IsAuthenticated => _currentUser != null;

    public void SetAuthStateChangedCallback(Action<UserWithPassword?> callback)
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

        var users = await GetUsersAsync();

        if (users.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
            return (false, "El email ya está registrado");

        if (users.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)))
            return (false, "El nombre de usuario ya está en uso");

        var user = new UserWithPassword
        {
            Username = username.Trim(),
            Email = email.Trim().ToLower(),
            PasswordHash = HashPassword(password)
        };

        users.Add(user);
        await SaveUsersAsync(users);

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

        var users = await GetUsersAsync();
        var user = users.FirstOrDefault(u => 
            u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

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
            var json = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", CURRENT_USER_KEY);
            if (!string.IsNullOrEmpty(json))
            {
                _currentUser = JsonSerializer.Deserialize<UserWithPassword>(json);
                _isInitialized = true;
                return _currentUser != null;
            }
        }
        catch { }

        _isInitialized = true;
        return false;
    }

    private async Task<List<UserWithPassword>> GetUsersAsync()
    {
        try
        {
            var json = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", USERS_KEY);
            if (string.IsNullOrEmpty(json))
                return new List<UserWithPassword>();

            return JsonSerializer.Deserialize<List<UserWithPassword>>(json) ?? new List<UserWithPassword>();
        }
        catch
        {
            return new List<UserWithPassword>();
        }
    }

    private async Task SaveUsersAsync(List<UserWithPassword> users)
    {
        var json = JsonSerializer.Serialize(users);
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", USERS_KEY, json);
    }

    private async Task SaveCurrentUserAsync(UserWithPassword user)
    {
        var json = JsonSerializer.Serialize(user);
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", CURRENT_USER_KEY, json);
    }

    public async Task<List<UserInfo>> SearchUsersAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm) || searchTerm.Length < 2)
            return new List<UserInfo>();

        var users = await GetUsersAsync();
        var term = searchTerm.ToLower();

        return users
            .Where(u => u.Username.ToLower().Contains(term) || u.Email.ToLower().Contains(term))
            .Where(u => _currentUser == null || u.Id != _currentUser.Id)
            .Take(10)
            .Select(u => new UserInfo { Id = u.Id, Username = u.Username, Email = u.Email })
            .ToList();
    }

    public class UserInfo
    {
        public string Id { get; set; } = "";
        public string Username { get; set; } = "";
        public string Email { get; set; } = "";
    }
}
