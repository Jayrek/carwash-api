using CarwashApi.DTO.Request;
using CarwashApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarwashApi.Controllers;

[ApiController]
[Route("api/admin/notifications")]
[Authorize(Roles = "admin")]
public class AdminNotificationsController : ControllerBase
{
    private readonly IPushNotificationService _pushNotifications;

    public AdminNotificationsController(IPushNotificationService pushNotifications)
    {
        _pushNotifications = pushNotifications;
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendAsync(
        [FromBody] SendPushNotificationRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _pushNotifications.SendToUserDevicesAsync(request, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Problem(detail: ex.Message, statusCode: StatusCodes.Status503ServiceUnavailable);
        }
    }
}
