public class RegisterDeviceRequest
{
    public string DeviceToken { get; set; } = null!;
    public string Platform { get; set; } = null!;
    public string? DeviceId { get; set; }
    public string? DeviceName { get; set; }
}