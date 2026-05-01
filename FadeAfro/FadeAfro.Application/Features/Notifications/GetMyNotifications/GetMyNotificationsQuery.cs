using MediatR;

namespace FadeAfro.Application.Features.Notifications.GetMyNotifications;

public record GetMyNotificationsQuery(Guid UserId) : IRequest<GetMyNotificationsResponse>;