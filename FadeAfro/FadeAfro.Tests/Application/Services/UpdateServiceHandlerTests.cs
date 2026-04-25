using FadeAfro.Application.Features.Services.UpdateService;
using FadeAfro.Domain.Entities;
using FadeAfro.Domain.Exceptions.Service;
using FadeAfro.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace FadeAfro.Tests.Application.Services;

public class UpdateServiceHandlerTests
{
    private readonly IServiceRepository _serviceRepository = Substitute.For<IServiceRepository>();
    private readonly UpdateServiceHandler _handler;

    public UpdateServiceHandlerTests()
    {
        _handler = new UpdateServiceHandler(_serviceRepository);
    }

    [Fact]
    public async Task Handle_ExistingService_UpdatesService()
    {
        var serviceId = Guid.NewGuid();
        var service = new Service(Guid.NewGuid(), "Old name", null, 100, TimeSpan.FromMinutes(20));
        var command = new UpdateServiceCommand(serviceId, "New name", "New desc", 200, TimeSpan.FromMinutes(45));

        _serviceRepository.GetByIdAsync(serviceId).Returns(service);

        await _handler.Handle(command, CancellationToken.None);

        await _serviceRepository.Received(1).UpdateAsync(Arg.Is<Service>(s =>
            s.Name == "New name" &&
            s.Description == "New desc" &&
            s.Price == 200 &&
            s.Duration == TimeSpan.FromMinutes(45)));
    }

    [Fact]
    public async Task Handle_ServiceNotFound_ThrowsServiceNotFoundException()
    {
        var command = new UpdateServiceCommand(Guid.NewGuid(), "Name", null, 100, TimeSpan.FromMinutes(30));
        _serviceRepository.GetByIdAsync(Arg.Any<Guid>()).Returns((Service?)null);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ServiceNotFoundException>();
        await _serviceRepository.DidNotReceive().UpdateAsync(Arg.Any<Service>());
    }
}
