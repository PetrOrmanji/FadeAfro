using FadeAfro.Application.Features.Services.GetMasterServices;
using FadeAfro.Domain.Entities;
using FadeAfro.Domain.Exceptions.MasterProfile;
using FadeAfro.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace FadeAfro.Tests.Application.Services;

public class GetMasterServicesHandlerTests
{
    private readonly IServiceRepository _serviceRepository = Substitute.For<IServiceRepository>();
    private readonly IMasterProfileRepository _masterProfileRepository = Substitute.For<IMasterProfileRepository>();
    private readonly GetMasterServicesHandler _handler;

    public GetMasterServicesHandlerTests()
    {
        _handler = new GetMasterServicesHandler(_serviceRepository, _masterProfileRepository);
    }

    [Fact]
    public async Task Handle_ExistingProfile_ReturnsMappedServices()
    {
        var masterProfileId = Guid.NewGuid();
        var profile = new MasterProfile(masterProfileId, null, null);
        var services = new List<Service>
        {
            new(masterProfileId, "Haircut", "Classic", 500, TimeSpan.FromMinutes(30)),
            new(masterProfileId, "Beard trim", null, 300, TimeSpan.FromMinutes(20)),
        };

        _masterProfileRepository.GetByIdAsync(masterProfileId).Returns(profile);
        _serviceRepository.GetByMasterProfileIdAsync(masterProfileId).Returns(services);

        var result = await _handler.Handle(new GetMasterServicesQuery(masterProfileId), CancellationToken.None);

        result.Services.Should().HaveCount(2);
        result.Services[0].Name.Should().Be("Haircut");
        result.Services[0].Price.Should().Be(500);
        result.Services[1].Name.Should().Be("Beard trim");
        result.Services[1].Description.Should().BeNull();
    }

    [Fact]
    public async Task Handle_MasterProfileNotFound_ThrowsMasterProfileNotFoundException()
    {
        _masterProfileRepository.GetByIdAsync(Arg.Any<Guid>()).Returns((MasterProfile?)null);

        var act = async () => await _handler.Handle(new GetMasterServicesQuery(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<MasterProfileNotFoundException>();
    }

    [Fact]
    public async Task Handle_NoServices_ReturnsEmptyList()
    {
        var masterProfileId = Guid.NewGuid();
        var profile = new MasterProfile(masterProfileId, null, null);

        _masterProfileRepository.GetByIdAsync(masterProfileId).Returns(profile);
        _serviceRepository.GetByMasterProfileIdAsync(masterProfileId).Returns([]);

        var result = await _handler.Handle(new GetMasterServicesQuery(masterProfileId), CancellationToken.None);

        result.Services.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_Service_MapsDurationCorrectly()
    {
        var masterProfileId = Guid.NewGuid();
        var profile = new MasterProfile(masterProfileId, null, null);
        var duration = TimeSpan.FromMinutes(90);
        var services = new List<Service> { new(masterProfileId, "Long cut", null, 1000, duration) };

        _masterProfileRepository.GetByIdAsync(masterProfileId).Returns(profile);
        _serviceRepository.GetByMasterProfileIdAsync(masterProfileId).Returns(services);

        var result = await _handler.Handle(new GetMasterServicesQuery(masterProfileId), CancellationToken.None);

        result.Services.Single().Duration.Should().Be(duration);
    }
}
