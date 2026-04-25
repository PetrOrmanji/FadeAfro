using FadeAfro.Domain.Entities;

namespace FadeAfro.Domain.Services;

public interface IJwtTokenService
{
    string GenerateToken(User user);
}
