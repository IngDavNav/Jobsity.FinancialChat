using Microsoft.AspNetCore.SignalR;

public class ChatHub : Hub<IChatClient>
{
    public async Task JoinRoom(string roomId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomId).ConfigureAwait(false);
    }

    public async Task LeaveRoom(string roomId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId).ConfigureAwait(false);
    }
}