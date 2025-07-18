using System;
using System.Threading.Tasks;
using MSH.Infrastructure.Entities;

namespace MSH.Web.Services;

public interface ICurrentUserService
{
    Task<User?> GetCurrentUserAsync();
    Task<string?> GetCurrentUserIdAsync();
    string? UserName { get; }
    bool IsAuthenticated { get; }
} 