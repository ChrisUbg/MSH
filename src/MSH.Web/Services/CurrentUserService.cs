using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using MSH.Infrastructure.Entities;
using MSH.Infrastructure.Services;

namespace MSH.Web.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserLookupService _userLookupService;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor, IUserLookupService userLookupService)
    {
        _httpContextAccessor = httpContextAccessor;
        _userLookupService = userLookupService;
    }

    public int? UserId => _userLookupService.GetCurrentUserId();

    public string? UserName => _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;

    public async Task<User?> GetCurrentUserAsync()
    {
        if (!UserId.HasValue)
        {
            return null;
        }

        return await _userLookupService.GetUserByIdAsync(UserId.Value);
    }
} 