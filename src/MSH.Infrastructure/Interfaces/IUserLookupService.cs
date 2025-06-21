using MSH.Infrastructure.Entities;

namespace MSH.Infrastructure.Interfaces;

public interface IUserLookupService
{
    String? GetCurrentUserId();
    Task<User?> GetUserByIdAsync(string userId);
    Task<User?> GetUserByUsernameAsync(string username);
    Task<User?> GetUserByEmailAsync(string email);
} 