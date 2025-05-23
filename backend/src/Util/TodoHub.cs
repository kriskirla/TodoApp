using Microsoft.AspNetCore.SignalR;

namespace TodoApp.Util;

public class TodoHub : Hub
{
    public async Task JoinListGroup(string listId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, listId);
    }

    public async Task LeaveListGroup(string listId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, listId);
    }
}
