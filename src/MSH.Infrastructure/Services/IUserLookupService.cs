using System.Threading.Tasks;
using MSH.Infrastructure.Entities;

namespace MSH.Infrastructure.Services;

public interface IUserLookupService
{
    Task<User?> GetUserByIdAsync(int userId);
    Task<User?> GetUserByUsernameAsync(string username);
    Task<User?> GetUserByEmailAsync(string email);
    int? GetCurrentUserId();
} 