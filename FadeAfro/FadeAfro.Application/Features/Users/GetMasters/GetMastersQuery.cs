using MediatR;

namespace FadeAfro.Application.Features.Users.GetMasters;

public record GetMastersQuery :  IRequest<GetMastersResponse>;