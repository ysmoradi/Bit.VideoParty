using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();

var app = builder.Build();

app.MapPost("/api/toggle", async (IHubContext<VideoPartyHub> videoPartyHubContext, string group, string senderConnectionId) =>
{
    await videoPartyHubContext.Clients.GroupExcept(group, senderConnectionId).SendAsync("Toggle");
});

app.MapHub<VideoPartyHub>("video-party-hub");

app.Run();


class VideoPartyHub : Hub
{
    public async Task AddToGroup(string group)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, group);
    }
}