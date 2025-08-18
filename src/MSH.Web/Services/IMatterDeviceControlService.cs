using System.Threading.Tasks;

namespace MSH.Web.Services;

public interface IMatterDeviceControlService
{
    Task<bool> ToggleDeviceAsync(string nodeId);
    Task<bool> TurnOnDeviceAsync(string nodeId);
    Task<bool> TurnOffDeviceAsync(string nodeId);
    Task<string?> GetDeviceStateAsync(string nodeId);
    Task<bool> IsDeviceOnlineAsync(string nodeId);
}
