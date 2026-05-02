using FadeAfro.Domain.Enums;
using FadeAfro.Domain.Exceptions.User;

namespace FadeAfro.Domain.Entities;

public class User : Entity
{
    public long TelegramId { get; private set; }
    public string FirstName { get; private set; }
    public string? LastName { get; private set; }
    public string? Username { get; private set; }
    public List<Role> Roles { get; private set; }

    public ICollection<Appointment> Appointments { get; private set; } = [];
    public ICollection<Notification> Notifications { get; private set; } = [];

    public User(long telegramId, string firstName, string? lastName, string? username, List<Role> roles)
    {
        if (telegramId <= 0)
            throw new InvalidTelegramIdException();

        if (string.IsNullOrWhiteSpace(firstName))
            throw new InvalidFirstNameException();

        if (roles == null || roles.Count == 0)
            throw new EmptyRolesException();

        TelegramId = telegramId;
        FirstName = firstName;
        LastName = lastName;
        Username = username;
        Roles = roles;
    }
    
    public string GetFullName() =>
        string.IsNullOrWhiteSpace(LastName) ? FirstName : $"{FirstName} {LastName}";

    public void Update(string firstName, string? lastName, string? username)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new InvalidFirstNameException();

        FirstName = firstName;
        LastName = lastName;
        Username = username;
    }
    
    public void UpdateUserName(string? username)
    {
        Username = username;
    }

    public void UpdateFullName(string firstName, string? lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new InvalidFirstNameException();

        FirstName = firstName;
        LastName = lastName;
    }

    public void AssignMasterRole()
    {
        if (Roles.Contains(Role.Master))
            throw new UserAlreadyMasterException();

        if (Roles.Contains(Role.Owner))
            Roles = [Role.Owner, Role.Master];
        else
            Roles = [Role.Master];
    }

    public void RevokeMasterRole()
    {
        if (!Roles.Contains(Role.Master))
            throw new UserNotMasterException();

        if (Roles.Contains(Role.Owner))
            Roles = [Role.Owner];
        else
            Roles = [Role.Client];
    }
}
