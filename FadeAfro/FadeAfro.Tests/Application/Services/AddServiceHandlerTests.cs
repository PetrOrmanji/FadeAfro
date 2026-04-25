using FadeAfro.Application.Features.Services.AddService;
using FadeAfro.Domain.Entities;
using FadeAfro.Domain.Exceptions.MasterProfile;
using FadeAfro.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace FadeAfro.Tests.Application.Services;

public class AddServiceHandlerTests
{
    private readonly IServiceRepository _serviceRepository = Substitute.For<IServiceRepository>();
    private readonly IMasterProfileRepository _masterProfileRepository = Substitute.For<IMasterProfileRepository>();
    private readonly AddServiceHandler _handler;

    public AddServiceHandlerTests()
    {
        _handler = new AddServiceHandler(_serviceRepository, _masterProfileRepository);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesAndReturnsResponse()
    {
        var masterProfileId = Guid.NewGuid();
        var profile = new MasterProfile(masterProfileId, null, null);
        var command = new AddServiceCommand(masterProfileId, "Haircut", "Classic cut", 500, TimeSpan.FromMinutes(30));

        _masterProfileRepository.GetByIdAsync(masterProfileId).Returns(profile);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Id.Should().NotBeEmpty();
        await _serviceRepository.Received(1).AddAsync(Arg.Is<Service>(s =>
            s.MasterProfileId == masterProfileId &&
            s.Name == "Haircut" &&
            s.Price == 500 &&
            s.Duration == TimeSpan.FromMinutes(30)));
    }

    [Fact]
    public async Task Handle_MasterProfileNotFound_ThrowsMasterProfileNotFoundException()
    {
        var command = new AddServiceCommand(Guid.NewGuid(), "Haircut", null, 500, TimeSpan.FromMinutes(30));
        _masterProfileRepository.GetByIdAsync(Arg.Any<Guid>()).Returns((MasterProfile?)null);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<MasterProfileNotFoundException>();
        await _serviceRepository.DidNotReceive().AddAsync(Arg.Any<Service>());
    }

    [Fact]
    public async Task Handle_NullDescription_CreatesService()
    {
        var masterProfileId = Guid.NewGuid();
        var profile = new MasterProfile(masterProfileId, null, null);
        var command = new AddServiceCommand(masterProfileId, "Haircut", null, 500, TimeSpan.FromMinutes(30));

        _masterProfileRepository.GetByIdAsync(masterProfileId).Returns(profile);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Id.Should().NotBeEmpty();
        await _serviceRepository.Received(1).AddAsync(Arg.Is<Service>(s => s.Description == null));
    }
}
