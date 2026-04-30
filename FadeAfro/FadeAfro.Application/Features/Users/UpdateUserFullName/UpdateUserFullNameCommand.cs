using MediatR;

namespace FadeAfro.Application.Features.Users.UpdateUserFullName;

public record UpdateUserFullNameCommand(
    Guid UserId,
    string FirstName,
    string? LastName) : IRequest;
