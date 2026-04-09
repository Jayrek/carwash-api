namespace CarwashApi.Models;

public class NotificationDelivery
{
    public Guid Id { get; set; }

    public Guid NotificationId { get; set; }
    public Guid UserDeviceId { get; set; }

    public DeliveryStatus Status { get; set; } = DeliveryStatus.Pending;
    public string? Response { get; set; }

    public DateTime SentAt { get; set; }

    public Notification Notification { get; set; } = null!;
    public UserDevice UserDevice { get; set; } = null!;
}