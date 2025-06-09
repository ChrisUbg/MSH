using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;

namespace MSH.Web.Components.Account;

public class IdentityUserAccessor
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly AuthenticationStateProvider _authenticationStateProvider;

    public IdentityUserAccessor(
        UserManager<IdentityUser> userManager,
        AuthenticationStateProvider authenticationStateProvider)
    {
        _userManager = userManager;
        _authenticationStateProvider = authenticationStateProvider;
    }

    public async Task<IdentityUser?> GetRequiredUserAsync()
    {
        var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        if (user.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        return await _userManager.GetUserAsync(user);
    }
} 