namespace CarwashApi.Models;

public class UserDevice
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    public string DeviceToken { get; set; } = null!;
    public string Platform { get; set; } = null!; // or enum

    public string? DeviceId { get; set; }
    public string? DeviceName { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? LastUsedAt { get; set; }

    public User User { get; set; } = null!;
}