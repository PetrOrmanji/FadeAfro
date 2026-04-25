using FadeAfro.Domain.Entities;
using FadeAfro.Domain.Exceptions.Service;
using FluentAssertions;

namespace FadeAfro.Tests.Domain;

public class ServiceTests
{
    private static readonly Guid MasterProfileId = Guid.NewGuid();

    [Fact]
    public void Constructor_ValidData_CreatesService()
    {
        var service = new Service(MasterProfileId, "Haircut", "Classic haircut", 500, TimeSpan.FromMinutes(30));

        service.MasterProfileId.Should().Be(MasterProfileId);
        service.Name.Should().Be("Haircut");
        service.Description.Should().Be("Classic haircut");
        service.Price.Should().Be(500);
        service.Duration.Should().Be(TimeSpan.FromMinutes(30));
        service.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Constructor_NullDescription_CreatesService()
    {
        var service = new Service(MasterProfileId, "Haircut", null, 500, TimeSpan.FromMinutes(30));

        service.Description.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_EmptyName_ThrowsInvalidServiceNameException(string name)
    {
        var act = () => new Service(MasterProfileId, name, null, 500, TimeSpan.FromMinutes(30));

        act.Should().Throw<InvalidServiceNameException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Constructor_InvalidPrice_ThrowsInvalidServicePriceException(int price)
    {
        var act = () => new Service(MasterProfileId, "Haircut", null, price, TimeSpan.FromMinutes(30));

        act.Should().Throw<InvalidServicePriceException>();
    }

    [Fact]
    public void Constructor_ZeroDuration_ThrowsInvalidServiceDurationException()
    {
        var act = () => new Service(MasterProfileId, "Haircut", null, 500, TimeSpan.Zero);

        act.Should().Throw<InvalidServiceDurationException>();
    }

    [Fact]
    public void Constructor_NegativeDuration_ThrowsInvalidServiceDurationException()
    {
        var act = () => new Service(MasterProfileId, "Haircut", null, 500, TimeSpan.FromMinutes(-10));

        act.Should().Throw<InvalidServiceDurationException>();
    }

    [Fact]
    public void Update_ValidData_UpdatesService()
    {
        var service = new Service(MasterProfileId, "Haircut", null, 500, TimeSpan.FromMinutes(30));

        service.Update("Beard trim", "Nice trim", 300, TimeSpan.FromMinutes(20));

        service.Name.Should().Be("Beard trim");
        service.Description.Should().Be("Nice trim");
        service.Price.Should().Be(300);
        service.Duration.Should().Be(TimeSpan.FromMinutes(20));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_EmptyName_ThrowsInvalidServiceNameException(string name)
    {
        var service = new Service(MasterProfileId, "Haircut", null, 500, TimeSpan.FromMinutes(30));

        var act = () => service.Update(name, null, 500, TimeSpan.FromMinutes(30));

        act.Should().Throw<InvalidServiceNameException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Update_InvalidPrice_ThrowsInvalidServicePriceException(int price)
    {
        var service = new Service(MasterProfileId, "Haircut", null, 500, TimeSpan.FromMinutes(30));

        var act = () => service.Update("Haircut", null, price, TimeSpan.FromMinutes(30));

        act.Should().Throw<InvalidServicePriceException>();
    }

    [Fact]
    public void Update_InvalidDuration_ThrowsInvalidServiceDurationException()
    {
        var service = new Service(MasterProfileId, "Haircut", null, 500, TimeSpan.FromMinutes(30));

        var act = () => service.Update("Haircut", null, 500, TimeSpan.Zero);

        act.Should().Throw<InvalidServiceDurationException>();
    }
}
