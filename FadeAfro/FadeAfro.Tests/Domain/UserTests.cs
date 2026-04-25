using FadeAfro.Domain.Entities;
using FadeAfro.Domain.Enums;
using FadeAfro.Domain.Exceptions.User;
using FluentAssertions;

namespace FadeAfro.Tests.Domain;

public class UserTests
{
    [Fact]
    public void Constructor_ValidData_CreatesUser()
    {
        var user = new User(123456789, "Ivan", "Petrov", "ivanp", [Role.Client]);

        user.TelegramId.Should().Be(123456789);
        user.FirstName.Should().Be("Ivan");
        user.LastName.Should().Be("Petrov");
        user.Username.Should().Be("ivanp");
        user.Roles.Should().ContainSingle(r => r == Role.Client);
        user.Id.Should().NotBeEmpty();
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Constructor_NullableFieldsAreNull_CreatesUser()
    {
        var user = new User(123456789, "Ivan", null, null, [Role.Client]);

        user.LastName.Should().BeNull();
        user.Username.Should().BeNull();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-999)]
    public void Constructor_InvalidTelegramId_ThrowsInvalidTelegramIdException(long telegramId)
    {
        var act = () => new User(telegramId, "Ivan", null, null, [Role.Client]);

        act.Should().Throw<InvalidTelegramIdException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_EmptyFirstName_ThrowsInvalidFirstNameException(string firstName)
    {
        var act = () => new User(123456789, firstName, null, null, [Role.Client]);

        act.Should().Throw<InvalidFirstNameException>();
    }

    [Fact]
    public void Constructor_NullRoles_ThrowsEmptyRolesException()
    {
        var act = () => new User(123456789, "Ivan", null, null, null!);

        act.Should().Throw<EmptyRolesException>();
    }

    [Fact]
    public void Constructor_EmptyRoles_ThrowsEmptyRolesException()
    {
        var act = () => new User(123456789, "Ivan", null, null, []);

        act.Should().Throw<EmptyRolesException>();
    }

    [Fact]
    public void Constructor_MultipleRoles_CreatesUserWithAllRoles()
    {
        var user = new User(123456789, "Ivan", null, null, [Role.Master, Role.Owner]);

        user.Roles.Should().HaveCount(2);
        user.Roles.Should().Contain(Role.Master);
        user.Roles.Should().Contain(Role.Owner);
    }
}
