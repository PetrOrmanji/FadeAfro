using MediatR;

namespace FadeAfro.Application.Features.Notifications.GetMyUnreadNotificationsCount;

public record GetMyUnreadNotificationsCountQuery(Guid UserId) : IRequest<GetMyUnreadNotificationsCountResponse>;