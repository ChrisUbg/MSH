namespace MSH.Web.Interfaces;

public interface ICurrentUserService
{
    Task<string> GetCurrentUserIdAsync();
} 