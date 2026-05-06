using FadeAfro.Application.Features.Notifications.GetMyUnreadNotifications;
using FadeAfro.Application.Features.Notifications.GetMyUnreadNotificationsCount;
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
public class NotificationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotificationsController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpGet("get/me/count/unread")]
    [Authorize]
    [SwaggerOperation(Summary = "Get count of my unread notifications")]
    public async Task<IActionResult> GetCountOfMyUnreadNotifications()
    {
        var getMyUnreadNotificationsCountQuery = new GetMyUnreadNotificationsCountQuery(
            User.GetUserId());
        
        var response = await _mediator.Send(getMyUnreadNotificationsCountQuery);
        return Ok(response);
    }

    [HttpGet("get/me/unread")]
    [Authorize]
    [SwaggerOperation(Summary = "Get my unread notifications")]
    public async Task<IActionResult> GetMyUnreadNotifications()
    {
        var getMyUnreadNotificationsQuery = new GetMyUnreadNotificationsQuery(
            User.GetUserId());
        
        var response = await _mediator.Send(getMyUnreadNotificationsQuery);
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