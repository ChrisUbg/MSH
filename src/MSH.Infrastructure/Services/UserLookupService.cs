using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using MSH.Infrastructure.Entities;
using MSH.Infrastructure.Interfaces;

namespace MSH.Infrastructure.Services;

public class UserLookupService : IUserLookupService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserLookupService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public String? GetCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return null;
        return userIdClaim.Value ?? "";
    }

    public Task<User?> GetUserByIdAsync(string userId)
    {
        // This will be implemented by the ApplicationDbContext
        throw new NotImplementedException();
    }

    public Task<User?> GetUserByUsernameAsync(string username)
    {
        // This will be implemented by the ApplicationDbContext
        throw new NotImplementedException();
    }

    public Task<User?> GetUserByEmailAsync(string email)
    {
        // This will be implemented by the ApplicationDbContext
        throw new NotImplementedException();
    }
} 