using System.Threading.Tasks;
using MSH.Infrastructure.Entities;

namespace MSH.Infrastructure.Services;

public interface IUserLookupService
{
    Task<User?> GetUserByIdAsync(Guid userId);
    Task<User?> GetUserByUsernameAsync(string username);
    Task<User?> GetUserByEmailAsync(string email);
    Guid? GetCurrentUserId();
} 