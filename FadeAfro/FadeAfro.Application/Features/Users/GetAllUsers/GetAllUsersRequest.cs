namespace FadeAfro.Application.Features.Users.GetAllUsers;

public record GetAllUsersRequest(
    int Page = 1,
    int PageSize = 20,
    string? Search = null);
