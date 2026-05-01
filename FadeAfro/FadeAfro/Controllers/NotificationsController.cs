using FadeAfro.Application.Features.Notifications.GetMyNotifications;
using FadeAfro.Application.Features.Notifications.MarkMyNotificationAsRead;
using FadeAfro.Application.Features.Notifications.MarkMyNotificationsAsRead;
using FadeAfro.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace FadeAfro.Controllers;

[ApiController]
[Route("api/notifications")]
[Tags("Notifications")]
public class NotificationsController : Controller
{
    private readonly IMediator _mediator;

    public NotificationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("get/me/all")]
    [Authorize]
    [SwaggerOperation(Summary = "Get my notifications")]
    public async Task<IActionResult> GetMyNotifications()
    {
        var getMyNotificationsQuery = new GetMyNotificationsQuery(
            User.GetUserId());
        
        var response = await _mediator.Send(getMyNotificationsQuery);
        return Ok(response);
    }
    
    [HttpPut("read/me/all")]
    [Authorize]
    [SwaggerOperation(Summary = "Read my notifications")]
    public async Task<IActionResult> ReadMyNotifications()
    {
        var markMyNotificationsAsReadCommand = new MarkMyNotificationsAsReadCommand(
            User.GetUserId());
        
        await _mediator.Send(markMyNotificationsAsReadCommand);
        return Ok();
    }
    
    [HttpPut("read/me/{notificationId:guid}")]
    [Authorize]
    [SwaggerOperation(Summary = "Read my notification")]
    public async Task<IActionResult> ReadMyNotification(Guid notificationId)
    {
        var markMyNotificationAsReadCommand = new MarkMyNotificationAsReadCommand(
            User.GetUserId(),
            notificationId);
        
        await _mediator.Send(markMyNotificationAsReadCommand);
        return Ok();
    }
}