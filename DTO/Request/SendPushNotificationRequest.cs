using System.ComponentModel.DataAnnotations;

namespace CarwashApi.DTO.Request;

public class SendPushNotificationRequest
{
    [Required]
    [MinLength(1)]
    public List<Guid> TargetUserIds { get; set; } = [];

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = null!;

    [Required]
    [MaxLength(4000)]
    public string Body { get; set; } = null!;

    [MaxLength(64)]
    public string? Type { get; set; }

    /// <summary>FCM data payload; values must be strings.</summary>
    public Dictionary<string, string>? Data { get; set; }
}
