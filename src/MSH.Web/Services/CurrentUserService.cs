using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using MSH.Infrastructure.Entities;
using MSH.Infrastructure.Data;
using System.Security.Claims;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Components.Authorization;
using MSH.Web.Interfaces;

namespace MSH.Web.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly AuthenticationStateProvider _authenticationStateProvider;

    public CurrentUserService(
        UserManager<IdentityUser> userManager,
        ApplicationDbContext context,
        AuthenticationStateProvider authenticationStateProvider)
    {
        _userManager = userManager;
        _context = context;
        _authenticationStateProvider = authenticationStateProvider;
    }

    public string? UserId
    {
        get
        {
            var userIdClaim = _userManager.GetUserId(null);
            return userIdClaim;
        }
    }

    public string? UserName => _userManager.Users.FirstOrDefault()?.UserName;

    public bool IsAuthenticated => _userManager.Users.Any();

    public async Task<User?> GetCurrentUserAsync()
    {
        var identityUser = _userManager.Users.FirstOrDefault();
        if (identityUser == null)
        {
            return null;
        }

        var user = await _context.ApplicationUsers
            .FirstOrDefaultAsync(u => u.UserName == identityUser.UserName);

        if (user == null)
        {
            user = new User
            {
                UserName = identityUser.UserName!,
                Email = identityUser.Email,
                IsActive = true,
                LastLogin = DateTime.UtcNow
            };
            _context.ApplicationUsers.Add(user);
            await _context.SaveChangesAsync();
        }

        return user;
    }

    public async Task<string?> GetCurrentUserIdAsync()
    {
        var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        
        if (user.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
        return userIdClaim?.Value;
    }
} 