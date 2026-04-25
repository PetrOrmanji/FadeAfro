using FadeAfro.Application.Features.MasterUnavailabilities.DeleteUnavailability;
using FadeAfro.Domain.Entities;
using FadeAfro.Domain.Exceptions.MasterUnavailability;
using FadeAfro.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace FadeAfro.Tests.Application.MasterUnavailabilities;

public class DeleteUnavailabilityHandlerTests
{
    private readonly IMasterUnavailabilityRepository _unavailabilityRepository = Substitute.For<IMasterUnavailabilityRepository>();
    private readonly DeleteUnavailabilityHandler _handler;

    public DeleteUnavailabilityHandlerTests()
    {
        _handler = new DeleteUnavailabilityHandler(_unavailabilityRepository);
    }

    [Fact]
    public async Task Handle_ExistingUnavailability_DeletesUnavailability()
    {
        var unavailabilityId = Guid.NewGuid();
        var unavailability = new MasterUnavailability(Guid.NewGuid(), new DateOnly(2026, 6, 1), null, null);

        _unavailabilityRepository.GetByIdAsync(unavailabilityId).Returns(unavailability);

        await _handler.Handle(new DeleteUnavailabilityCommand(unavailabilityId), CancellationToken.None);

        await _unavailabilityRepository.Received(1).DeleteAsync(unavailabilityId);
    }

    [Fact]
    public async Task Handle_UnavailabilityNotFound_ThrowsMasterUnavailabilityNotFoundException()
    {
        _unavailabilityRepository.GetByIdAsync(Arg.Any<Guid>()).Returns((MasterUnavailability?)null);

        var act = async () => await _handler.Handle(new DeleteUnavailabilityCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<MasterUnavailabilityNotFoundException>();
        await _unavailabilityRepository.DidNotReceive().DeleteAsync(Arg.Any<Guid>());
    }
}
