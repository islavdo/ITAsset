using System.Net.Http.Headers;
using System.Net.Http.Json;
using ITAssetAccounting.Shared.Dto;
using Microsoft.Net.Http.Headers;

namespace ITAssetAccounting.ReportService.Services;

public class EquipmentApiClient
{
    private readonly HttpClient _http;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public EquipmentApiClient(HttpClient http, IHttpContextAccessor httpContextAccessor)
    {
        _http = http;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<IReadOnlyCollection<EquipmentDto>> GetAllEquipmentAsync(CancellationToken ct = default)
    {
        var auth = _httpContextAccessor.HttpContext?.Request.Headers[HeaderNames.Authorization].ToString();
        if (!string.IsNullOrWhiteSpace(auth))
        {
            _http.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(auth);
        }
        return await _http.GetFromJsonAsync<IReadOnlyCollection<EquipmentDto>>("/api/equipment/all", ct)
            ?? Array.Empty<EquipmentDto>();
    }
}
