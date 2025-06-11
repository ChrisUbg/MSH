using MSH.Infrastructure.Entities;

namespace MSH.Infrastructure.Interfaces;

public interface IUserLookupService
{
    Guid? GetCurrentUserId();
    Task<User?> GetUserByIdAsync(Guid userId);
    Task<User?> GetUserByUsernameAsync(string username);
    Task<User?> GetUserByEmailAsync(string email);
} 