using Microsoft.AspNetCore.SignalR.Client;

namespace ITAssetAccounting.WebClient.Services;

public class RealtimeClient : IAsyncDisposable
{
    private readonly AuthState _auth;
    private HubConnection? _equipmentHub;
    private HubConnection? _maintenanceHub;

    public event Action? EquipmentChanged;
    public event Action? MaintenanceChanged;

    public RealtimeClient(AuthState auth)
    {
        _auth = auth;
    }

    public async Task StartAsync(string gatewayBaseUrl)
    {
        _equipmentHub = new HubConnectionBuilder()
            .WithUrl(new Uri(new Uri(gatewayBaseUrl), "hubs/equipment"), options => options.AccessTokenProvider = () => Task.FromResult(_auth.AccessToken))
            .WithAutomaticReconnect()
            .Build();
        _maintenanceHub = new HubConnectionBuilder()
            .WithUrl(new Uri(new Uri(gatewayBaseUrl), "hubs/maintenance"), options => options.AccessTokenProvider = () => Task.FromResult(_auth.AccessToken))
            .WithAutomaticReconnect()
            .Build();
        _equipmentHub.On("equipmentCreated", () => EquipmentChanged?.Invoke());
        _equipmentHub.On("equipmentUpdated", () => EquipmentChanged?.Invoke());
        _equipmentHub.On("equipmentAssigned", () => EquipmentChanged?.Invoke());
        _maintenanceHub.On("maintenanceCreated", () => MaintenanceChanged?.Invoke());
        _maintenanceHub.On("maintenanceStatusChanged", () => MaintenanceChanged?.Invoke());
        await _equipmentHub.StartAsync();
        await _maintenanceHub.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_equipmentHub is not null) await _equipmentHub.DisposeAsync();
        if (_maintenanceHub is not null) await _maintenanceHub.DisposeAsync();
    }
}
