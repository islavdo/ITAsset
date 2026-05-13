using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ITAssetAccounting.EquipmentService.Hubs;

[Authorize]
public class EquipmentHub : Hub
{
    public Task SubscribeToEquipment(int equipmentId) => Groups.AddToGroupAsync(Context.ConnectionId, $"equipment:{equipmentId}");
    public Task UnsubscribeFromEquipment(int equipmentId) => Groups.RemoveFromGroupAsync(Context.ConnectionId, $"equipment:{equipmentId}");
}
