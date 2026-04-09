namespace CarwashApi.Models;

public class Notification
{
    public Guid Id { get; set; }

    public Guid? UserId { get; set; }

    public string Title { get; set; } = null!;
    public string Body { get; set; } = null!;

    public string? Type { get; set; }
    public string? Data { get; set; } // JSON string

    public bool IsBroadcast { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? SentAt { get; set; }

    public ICollection<NotificationDelivery> Deliveries { get; set; } = new List<NotificationDelivery>();
}