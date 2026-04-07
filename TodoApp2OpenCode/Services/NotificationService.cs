using Microsoft.EntityFrameworkCore;
using TodoApp2OpenCode.Data;
using TodoApp2OpenCode.Models;

namespace TodoApp2OpenCode.Services;

public class NotificationService : INotificationService
{
    private readonly IFlowBoardDbContextFactory _contextFactory;

    public NotificationService(IFlowBoardDbContextFactory contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task CreateAsync(string userId, string title, string message, string? navigateTo = null)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var notification = new Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            NavigateTo = navigateTo,
            IsRead = false,
            CreatedAt = DateTime.Now
        };

        context.Notifications.Add(notification);
        await context.SaveChangesAsync();
    }

    public async Task<int> GetUnreadCountAsync(string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        return await context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .CountAsync();
    }

    public async Task<List<Notification>> GetAllAsync(string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        return await context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(50)
            .ToListAsync();
    }

    public async Task MarkAsReadAsync(string notificationId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var notification = await context.Notifications.FindAsync(notificationId);
        if (notification != null)
        {
            notification.IsRead = true;
            await context.SaveChangesAsync();
        }
    }

    public async Task MarkAllAsReadAsync(string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var notifications = await context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
        }

        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(string notificationId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var notification = await context.Notifications.FindAsync(notificationId);
        if (notification != null)
        {
            context.Notifications.Remove(notification);
            await context.SaveChangesAsync();
        }
    }

    public async Task DeleteAllAsync(string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var notifications = await context.Notifications
            .Where(n => n.UserId == userId)
            .ToListAsync();

        context.Notifications.RemoveRange(notifications);
        await context.SaveChangesAsync();
    }
}