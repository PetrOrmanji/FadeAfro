using FadeAfro.Domain.Enums;
using FadeAfro.Domain.Exceptions;

namespace FadeAfro.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public long TelegramId { get; private set; }
    public string FirstName { get; private set; }
    public string? LastName { get; private set; }
    public string? Username { get; private set; }
    public List<Role> Roles { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public User(long telegramId, string firstName, string? lastName, string? username, List<Role> roles)
    {
        if (telegramId <= 0)
            throw new InvalidTelegramIdException();

        if (string.IsNullOrWhiteSpace(firstName))
            throw new InvalidFirstNameException();

        if (roles == null || roles.Count == 0)
            throw new EmptyRolesException();

        Id = Guid.NewGuid();
        TelegramId = telegramId;
        FirstName = firstName;
        LastName = lastName;
        Username = username;
        Roles = roles;
        CreatedAt = DateTime.UtcNow;
    }
}
