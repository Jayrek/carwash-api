using System.Text.Json;
using CarwashApi.Data;
using CarwashApi.DTO.Request;
using CarwashApi.DTO.Response;
using CarwashApi.Models;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NotificationEntity = CarwashApi.Models.Notification;

namespace CarwashApi.Services;

public class PushNotificationService : IPushNotificationService
{
    public const int FcmMaxTokensPerMulticast = 500;

    private readonly AppDbContext _db;
    private readonly ILogger<PushNotificationService> _logger;

    public PushNotificationService(AppDbContext db, ILogger<PushNotificationService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<SendPushNotificationResponse> SendToUserDevicesAsync(
        SendPushNotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (FirebaseApp.DefaultInstance is null)
        {
            throw new InvalidOperationException(
                "Firebase is not initialized. Set GOOGLE_APPLICATION_CREDENTIALS to a valid service account JSON path.");
        }

        var userIds = request.TargetUserIds.Distinct().ToList();
        if (userIds.Count == 0)
            throw new ArgumentException("At least one target user is required.", nameof(request));

        var devices = await _db.UserDevices
            .Where(d => userIds.Contains(d.UserId) && d.IsActive && d.DeviceToken.Length > 0)
            .ToListAsync(cancellationToken);

        var dataJson = request.Data is null || request.Data.Count == 0
            ? null
            : JsonSerializer.Serialize(request.Data);

        var notificationEntity = new NotificationEntity
        {
            Id = Guid.NewGuid(),
            UserId = userIds.Count == 1 ? userIds[0] : null,
            Title = request.Title,
            Body = request.Body,
            Type = request.Type,
            Data = dataJson,
            IsBroadcast = false,
            CreatedAt = DateTime.UtcNow,
        };
        _db.Notifications.Add(notificationEntity);

        var deliveries = new List<NotificationDelivery>();
        foreach (var device in devices)
        {
            var delivery = new NotificationDelivery
            {
                Id = Guid.NewGuid(),
                NotificationId = notificationEntity.Id,
                UserDeviceId = device.Id,
                Status = DeliveryStatus.Pending,
                SentAt = DateTime.UtcNow,
            };
            deliveries.Add(delivery);
            _db.NotificationDeliveries.Add(delivery);
        }

        await _db.SaveChangesAsync(cancellationToken);

        var response = new SendPushNotificationResponse
        {
            NotificationId = notificationEntity.Id,
            TargetedDeviceCount = devices.Count,
        };

        if (devices.Count == 0)
        {
            notificationEntity.SentAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
            return response;
        }

        var fcmData = BuildFcmData(request);
        var pairs = devices.Zip(deliveries, (device, delivery) => (device, delivery)).ToList();

        var deactivatedIds = new HashSet<Guid>();
        var success = 0;
        var failure = 0;

        foreach (var chunk in pairs.Chunk(FcmMaxTokensPerMulticast))
        {
            var chunkList = chunk.ToArray();
            var multicast = new MulticastMessage
            {
                Tokens = chunkList.Select(p => p.device.DeviceToken).ToList(),
                Notification = new FirebaseAdmin.Messaging.Notification
                {
                    Title = request.Title,
                    Body = request.Body,
                },
                Data = fcmData,
            };

            var batch = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(
                multicast,
                cancellationToken);

            for (var i = 0; i < batch.Responses.Count; i++)
            {
                var (device, delivery) = chunkList[i];
                var sendResponse = batch.Responses[i];

                if (sendResponse.IsSuccess)
                {
                    success++;
                    delivery.Status = DeliveryStatus.Sent;
                    delivery.Response = TruncateResponse(sendResponse.MessageId);
                }
                else
                {
                    failure++;
                    delivery.Status = DeliveryStatus.Failed;
                    var ex = sendResponse.Exception;
                    delivery.Response = TruncateResponse(ex?.Message);
                    if (ex is FirebaseMessagingException fcmEx && ShouldDeactivateDevice(fcmEx))
                    {
                        device.IsActive = false;
                        device.UpdatedAt = DateTime.UtcNow;
                        deactivatedIds.Add(device.Id);
                    }
                }
            }
        }

        notificationEntity.SentAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        response.SuccessCount = success;
        response.FailureCount = failure;
        response.DeactivatedDeviceCount = deactivatedIds.Count;

        _logger.LogInformation(
            "Push notification {NotificationId}: devices={Count}, ok={Ok}, fail={Fail}, deactivated={Off}",
            notificationEntity.Id,
            devices.Count,
            success,
            failure,
            deactivatedIds.Count);

        return response;
    }

    private static IReadOnlyDictionary<string, string>? BuildFcmData(SendPushNotificationRequest request)
    {
        if (request.Data is null || request.Data.Count == 0)
            return null;

        var dict = new Dictionary<string, string>(request.Data, StringComparer.Ordinal);
        if (!string.IsNullOrEmpty(request.Type))
            dict["type"] = request.Type;

        return dict;
    }

    private static bool ShouldDeactivateDevice(FirebaseMessagingException ex)
    {
        return ex.MessagingErrorCode switch
        {
            MessagingErrorCode.Unregistered => true,
            MessagingErrorCode.SenderIdMismatch => true,
            MessagingErrorCode.InvalidArgument =>
                ex.Message.Contains("registration-token-not-registered", StringComparison.OrdinalIgnoreCase)
                || ex.Message.Contains("invalid-registration-token", StringComparison.OrdinalIgnoreCase),
            _ => false,
        };
    }

    private static string? TruncateResponse(string? value, int maxLen = 2000)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return value.Length <= maxLen ? value : value[..maxLen];
    }
}
