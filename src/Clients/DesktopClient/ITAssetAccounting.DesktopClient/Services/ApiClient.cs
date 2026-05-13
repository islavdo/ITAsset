using System.Net.Http.Headers;
using System.Net.Http.Json;
using ITAssetAccounting.Shared.Dto;
using ITAssetAccounting.Shared.Paging;

namespace ITAssetAccounting.DesktopClient.Services;

public class ApiClient
{
    private readonly HttpClient _http;
    public string? AccessToken { get; private set; }

    public ApiClient(HttpClient http)
    {
        _http = http;
    }

    private void ApplyToken()
    {
        if (!string.IsNullOrWhiteSpace(AccessToken))
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
        }
    }

    public async Task<bool> LoginAsync(string email, string password)
    {
        var response = await _http.PostAsJsonAsync("identity/auth/login", new LoginRequest(email, password));
        if (!response.IsSuccessStatusCode) return false;
        var token = await response.Content.ReadFromJsonAsync<TokenResponse>();
        AccessToken = token?.AccessToken;
        return AccessToken is not null;
    }

    public async Task<PagedResult<EquipmentDto>?> GetEquipmentAsync(string search)
    {
        ApplyToken();
        return await _http.GetFromJsonAsync<PagedResult<EquipmentDto>>($"equipment/equipment?page=1&pageSize=50&search={Uri.EscapeDataString(search)}");
    }
}
