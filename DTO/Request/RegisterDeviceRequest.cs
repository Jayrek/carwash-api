using CarwashApi.Models;

public class RegisterDeviceRequest
{
    public string DeviceToken { get; set; } = null!;
    public DevicePlatform Platform { get; set; }
    public string? DeviceId { get; set; }
    public string? DeviceName { get; set; }
}