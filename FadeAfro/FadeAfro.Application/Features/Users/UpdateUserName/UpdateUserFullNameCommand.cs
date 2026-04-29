using MediatR;

namespace FadeAfro.Application.Features.Users.UpdateUserName;

public record UpdateUserFullNameCommand(
    Guid UserId,
    string FirstName,
    string? LastName) : IRequest;
