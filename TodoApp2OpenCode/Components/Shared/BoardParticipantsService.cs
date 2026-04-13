using Microsoft.AspNetCore.Components;
using TodoApp2OpenCode.Models;
using TodoApp2OpenCode.Services;

namespace TodoApp2OpenCode.Components.Shared;

public class BoardParticipantsService
{
    private readonly IBoardService _boardService;
    private readonly ILogService _logService;

    public BoardParticipantsService(IBoardService boardService, ILogService logService)
    {
        _boardService = boardService;
        _logService = logService;
    }

    public async Task AddParticipantsAsync(string boardId, TodoBoard board, List<(string Id, string Name)> selectedParticipants, string userName)
    {
        foreach (var (id, name) in selectedParticipants)
        {
            if (!board.Participants.ContainsKey(id))
            {
                await _boardService.AddParticipantAsync(boardId, id, name);
            }
        }

        await _logService.AddLogAsync(new LogItem
        {
            Message = $"Añade participantes {string.Join(", ", selectedParticipants.Select(x => x.Name))} del tablero {board.Name}",
            Action = DatabaseAction.Crear,
            BoardId = board.Id,
            User = userName
        });
    }

    public async Task RemoveParticipantAsync(string boardId, TodoBoard board, string participantId, List<TodoItem> allItems, Action onRemoved)
    {
        await _boardService.RemoveParticipantAsync(boardId, participantId);
        board.Participants.Remove(participantId);

        foreach (var item in allItems)
        {
            if (item.AssignedUsers != null && item.AssignedUsers.ContainsKey(participantId))
            {
                item.AssignedUsers.Remove(participantId);
            }
        }

        onRemoved();
    }

    public bool HasIncompleteSteps(string participantId, List<TodoItem> allItems)
    {
        return allItems
            .Where(item => item.AssignedUsers?.ContainsKey(participantId) == true)
            .Any(item => item.Steps != null && item.Steps.Any(step => !step.IsCompleted));
    }
}