using TodoApp2OpenCode.Models;

namespace TodoApp2OpenCode.Services;

public interface INotificationService
{
    Task CreateAsync(string userId, string title, string message, string? navigateTo = null);
    Task<(string, int)> GetUnreadCountAsync(string userId);
    Task<List<Notification>> GetAllAsync(string userId);
    Task MarkAsReadAsync(string notificationId);
    Task MarkAllAsReadAsync(string userId);
    Task DeleteAsync(string notificationId);
    Task DeleteAllAsync(string userId);
}