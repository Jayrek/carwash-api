using CarwashApi.DTO.Request;
using CarwashApi.DTO.Response;

namespace CarwashApi.Services;

public interface IPushNotificationService
{
    Task<SendPushNotificationResponse> SendToUserDevicesAsync(
        SendPushNotificationRequest request,
        CancellationToken cancellationToken = default);
}
