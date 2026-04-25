using FadeAfro.Application.Features.Services.DeleteService;
using FadeAfro.Domain.Entities;
using FadeAfro.Domain.Exceptions.Service;
using FadeAfro.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace FadeAfro.Tests.Application.Services;

public class DeleteServiceHandlerTests
{
    private readonly IServiceRepository _serviceRepository = Substitute.For<IServiceRepository>();
    private readonly DeleteServiceHandler _handler;

    public DeleteServiceHandlerTests()
    {
        _handler = new DeleteServiceHandler(_serviceRepository);
    }

    [Fact]
    public async Task Handle_ExistingService_DeletesService()
    {
        var serviceId = Guid.NewGuid();
        var service = new Service(Guid.NewGuid(), "Haircut", null, 500, TimeSpan.FromMinutes(30));

        _serviceRepository.GetByIdAsync(serviceId).Returns(service);

        await _handler.Handle(new DeleteServiceCommand(serviceId), CancellationToken.None);

        await _serviceRepository.Received(1).DeleteAsync(serviceId);
    }

    [Fact]
    public async Task Handle_ServiceNotFound_ThrowsServiceNotFoundException()
    {
        _serviceRepository.GetByIdAsync(Arg.Any<Guid>()).Returns((Service?)null);

        var act = async () => await _handler.Handle(new DeleteServiceCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<ServiceNotFoundException>();
        await _serviceRepository.DidNotReceive().DeleteAsync(Arg.Any<Guid>());
    }
}
