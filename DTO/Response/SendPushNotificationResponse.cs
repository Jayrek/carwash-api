namespace CarwashApi.DTO.Response;

public class SendPushNotificationResponse
{
    public Guid NotificationId { get; set; }
    public int TargetedDeviceCount { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public int DeactivatedDeviceCount { get; set; }
}
