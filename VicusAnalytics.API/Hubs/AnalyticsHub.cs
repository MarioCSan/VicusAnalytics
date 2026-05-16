using Microsoft.AspNetCore.SignalR;

namespace VicusAnalytics.API.Hubs;

public class AnalyticsHub : Hub
{
    public async Task JoinRoom(string room) =>
        await Groups.AddToGroupAsync(Context.ConnectionId, room);
}
