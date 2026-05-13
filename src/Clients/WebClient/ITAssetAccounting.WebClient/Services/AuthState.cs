using ITAssetAccounting.Shared.Dto;
using Microsoft.JSInterop;

namespace ITAssetAccounting.WebClient.Services;

public class AuthState
{
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
        AccessToken = await _js.InvokeAsync<string?>("localStorage.getItem", "access_token");
        RefreshToken = await _js.InvokeAsync<string?>("localStorage.getItem", "refresh_token");
    }

    public async Task SetAsync(TokenResponse token)
    {
        AccessToken = token.AccessToken;
        RefreshToken = token.RefreshToken;
        CurrentUser = token.User;
        await _js.InvokeVoidAsync("localStorage.setItem", "access_token", token.AccessToken);
        await _js.InvokeVoidAsync("localStorage.setItem", "refresh_token", token.RefreshToken);
        Changed?.Invoke();
    }

    public async Task LogoutAsync()
    {
        AccessToken = null;
        RefreshToken = null;
        CurrentUser = null;
        await _js.InvokeVoidAsync("localStorage.removeItem", "access_token");
        await _js.InvokeVoidAsync("localStorage.removeItem", "refresh_token");
        Changed?.Invoke();
    }
}
