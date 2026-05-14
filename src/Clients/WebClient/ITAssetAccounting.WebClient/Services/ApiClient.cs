using System.Net.Http.Headers;
using System.Net.Http.Json;
using ITAssetAccounting.Shared.Dto;
using ITAssetAccounting.Shared.Enums;
using ITAssetAccounting.Shared.Paging;

namespace ITAssetAccounting.WebClient.Services;

public class ApiClient
{
    private readonly HttpClient _http;
    private readonly AuthState _auth;

    public ApiClient(HttpClient http, AuthState auth)
    {
        _http = http;
        _auth = auth;
    }

    private void ApplyToken()
    {
        _http.DefaultRequestHeaders.Authorization = string.IsNullOrWhiteSpace(_auth.AccessToken)
            ? null
            : new AuthenticationHeaderValue("Bearer", _auth.AccessToken);
    }

    public async Task<TokenResponse?> LoginAsync(LoginRequest request)
    {
        var response = await _http.PostAsJsonAsync("identity/auth/login", request);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<TokenResponse>();
    }

    public async Task<PagedResult<EquipmentDto>?> GetEquipmentAsync(EquipmentFilter filter)
    {
        ApplyToken();
        var url = $"equipment/equipment?page={filter.Page}&pageSize={filter.PageSize}&search={Uri.EscapeDataString(filter.Search ?? string.Empty)}";
        if (filter.Status.HasValue) url += $"&status={filter.Status}";
        return await _http.GetFromJsonAsync<PagedResult<EquipmentDto>>(url);
    }

    public async Task<EquipmentDto?> CreateEquipmentAsync(EquipmentCreateRequest request)
    {
        ApplyToken();
        var response = await _http.PostAsJsonAsync("equipment/equipment", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<EquipmentDto>();
    }

    public async Task<IReadOnlyCollection<CategoryDto>> GetCategoriesAsync()
    {
        ApplyToken();
        return await _http.GetFromJsonAsync<IReadOnlyCollection<CategoryDto>>("equipment/categories") ?? Array.Empty<CategoryDto>();
    }

    public async Task<IReadOnlyCollection<LocationDto>> GetLocationsAsync()
    {
        ApplyToken();
        return await _http.GetFromJsonAsync<IReadOnlyCollection<LocationDto>>("equipment/locations") ?? Array.Empty<LocationDto>();
    }

    public async Task<DashboardDto?> GetDashboardAsync()
    {
        ApplyToken();
        return await _http.GetFromJsonAsync<DashboardDto>("reports/dashboard");
    }

    public async Task<PagedResult<MaintenanceRequestDto>?> GetMaintenanceAsync(MaintenanceFilter filter)
    {
        ApplyToken();
        var url = $"maintenance/maintenance?page={filter.Page}&pageSize={filter.PageSize}&search={Uri.EscapeDataString(filter.Search ?? string.Empty)}";
        return await _http.GetFromJsonAsync<PagedResult<MaintenanceRequestDto>>(url);
    }

    public async Task<MaintenanceRequestDto?> CreateMaintenanceAsync(MaintenanceCreateRequest request)
    {
        ApplyToken();
        var response = await _http.PostAsJsonAsync("maintenance/maintenance", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<MaintenanceRequestDto>();
    }
}
