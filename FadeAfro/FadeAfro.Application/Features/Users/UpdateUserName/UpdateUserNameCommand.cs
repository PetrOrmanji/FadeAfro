using MediatR;

namespace FadeAfro.Application.Features.Users.UpdateUserName;

public record UpdateUserNameCommand(
    Guid UserId,
    string FirstName,
    string? LastName) : IRequest<Unit>;
