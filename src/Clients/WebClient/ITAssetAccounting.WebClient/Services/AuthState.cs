using System.Text.Json;
using ITAssetAccounting.Shared.Dto;
using Microsoft.JSInterop;

namespace ITAssetAccounting.WebClient.Services;

public class AuthState
{
    private const string AccessTokenKey = "access_token";
    private const string RefreshTokenKey = "refresh_token";
    private const string CurrentUserKey = "current_user";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IJSRuntime _js;
    public string? AccessToken { get; private set; }
    public string? RefreshToken { get; private set; }
    public UserDto? CurrentUser { get; private set; }
    public event Action? Changed;

    public AuthState(IJSRuntime js)
    {
        _js = js;
    }

    public async Task LoadAsync()
    {
        AccessToken = await _js.InvokeAsync<string?>("localStorage.getItem", AccessTokenKey);
        RefreshToken = await _js.InvokeAsync<string?>("localStorage.getItem", RefreshTokenKey);
        var userJson = await _js.InvokeAsync<string?>("localStorage.getItem", CurrentUserKey);
        CurrentUser = string.IsNullOrWhiteSpace(userJson)
            ? null
            : JsonSerializer.Deserialize<UserDto>(userJson, JsonOptions);
    }

    public async Task SetAsync(TokenResponse token)
    {
        AccessToken = token.AccessToken;
        RefreshToken = token.RefreshToken;
        CurrentUser = token.User;
        await _js.InvokeVoidAsync("localStorage.setItem", AccessTokenKey, token.AccessToken);
        await _js.InvokeVoidAsync("localStorage.setItem", RefreshTokenKey, token.RefreshToken);
        await _js.InvokeVoidAsync("localStorage.setItem", CurrentUserKey, JsonSerializer.Serialize(token.User, JsonOptions));
        Changed?.Invoke();
    }

    public async Task LogoutAsync()
    {
        AccessToken = null;
        RefreshToken = null;
        CurrentUser = null;
        await _js.InvokeVoidAsync("localStorage.removeItem", AccessTokenKey);
        await _js.InvokeVoidAsync("localStorage.removeItem", RefreshTokenKey);
        await _js.InvokeVoidAsync("localStorage.removeItem", CurrentUserKey);
        Changed?.Invoke();
    }
}
