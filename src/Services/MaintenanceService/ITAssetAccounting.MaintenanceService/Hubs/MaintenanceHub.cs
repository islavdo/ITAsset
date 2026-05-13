using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ITAssetAccounting.MaintenanceService.Hubs;

[Authorize]
public class MaintenanceHub : Hub
{
    public Task SubscribeToRequest(int requestId) => Groups.AddToGroupAsync(Context.ConnectionId, $"maintenance:{requestId}");
    public Task UnsubscribeFromRequest(int requestId) => Groups.RemoveFromGroupAsync(Context.ConnectionId, $"maintenance:{requestId}");
}
