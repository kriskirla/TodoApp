using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace TodoApp.Util;

[Authorize]
public class TodoHub(ILogger<TodoHub> logger) : Hub
{
    public async Task JoinListGroup(string listId)
    {
        logger.LogInformation("Client joined group {Group}: {ConnId}", listId, Context.ConnectionId);
        await Groups.AddToGroupAsync(Context.ConnectionId, listId);
    }

    public async Task LeaveListGroup(string listId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, listId);
    }
}
