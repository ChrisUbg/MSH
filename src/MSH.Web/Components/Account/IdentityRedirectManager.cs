using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace MSH.Web.Components.Account;

public class IdentityRedirectManager
{
    private readonly NavigationManager _navigationManager;
    private readonly AuthenticationStateProvider _authenticationStateProvider;

    public IdentityRedirectManager(
        NavigationManager navigationManager,
        AuthenticationStateProvider authenticationStateProvider)
    {
        _navigationManager = navigationManager;
        _authenticationStateProvider = authenticationStateProvider;
    }

    public async Task RedirectToLoginAsync(string? returnUrl = null)
    {
        var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        if (authState.User.Identity?.IsAuthenticated != true)
        {
            var redirectUrl = "login";
            if (!string.IsNullOrEmpty(returnUrl))
            {
                redirectUrl += $"?returnUrl={Uri.EscapeDataString(returnUrl)}";
            }
            _navigationManager.NavigateTo(redirectUrl);
        }
    }

    public void RedirectToLogin(string? returnUrl = null)
    {
        var redirectUrl = "login";
        if (!string.IsNullOrEmpty(returnUrl))
        {
            redirectUrl += $"?returnUrl={Uri.EscapeDataString(returnUrl)}";
        }
        _navigationManager.NavigateTo(redirectUrl);
    }
} 