using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using MSH.Infrastructure.Entities;

namespace MSH.Infrastructure.Services;

public class UserLookupService : IUserLookupService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserLookupService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int? GetCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return null;
        return int.TryParse(userIdClaim.Value, out var userId) ? userId : null;
    }

    public Task<User?> GetUserByIdAsync(int userId)
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