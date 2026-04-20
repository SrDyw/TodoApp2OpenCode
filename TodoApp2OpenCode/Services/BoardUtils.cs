using Microsoft.AspNetCore.Mvc.Diagnostics;
using TodoApp2OpenCode.Models;

namespace TodoApp2OpenCode.Services;

public static class BoardUtils
{
    public static bool CheckPermission(TodoBoard board, string userId, Func<BoardPermissions, bool> hasPermission)
    {
        if (string.IsNullOrEmpty(userId))
            return false;

        if (board.User != userId)
        {
            if (!board.ParticipantPermissions.TryGetValue(userId, out var perms) || perms == null)
                return false;

            if (!hasPermission(perms))
                return false;
        }

        return true;
    }

    public static Dictionary<string, string> GetParticipantsDictionary(TodoBoard board)
    {
        var participants = new Dictionary<string, string>();
        
        if (board == null) return participants;
        
        participants[board.User] = board.OwnerName;
        
        if (board.Participants != null)
        {
            foreach (var participant in board.Participants)
            {
                participants[participant.Key] = participant.Value;
            }
        }
        
        return participants;
    }

    public static BoardPermissions GetUserPermissions(TodoBoard? board, string userId)
    {
        if (board == null) return new BoardPermissions();

        bool isOwner = board.User == userId;

        if (isOwner)
        {
            return new BoardPermissions
            {
                CanAddTasks = true,
                CanModifyTasks = true,
                CanDeleteTasks = true,
                CanViewCalendar = true,
                CanAddColumn = true,
                CanEditColumn = true,
                CanDeleteColumn = true
            };
        }

        if (board.ParticipantPermissions?.TryGetValue(userId, out var perms) == true)
            return perms;

        return new BoardPermissions();
    }

    public static bool IsUserOwnerOrParticipant(TodoBoard? board, string userId)
    {
        if (board == null) return false;
        return board.User == userId || (board.Participants?.ContainsKey(userId) ?? false);
    }
}