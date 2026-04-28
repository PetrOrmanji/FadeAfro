using FadeAfro.Application.Common;
using FadeAfro.Application.Common.Appointments;
using FadeAfro.Domain.Exceptions.User;
using FadeAfro.Domain.Repositories;
using MediatR;

namespace FadeAfro.Application.Features.Appointments.GetClientAppointments;

public class GetClientAppointmentsHandler : IRequestHandler<GetClientAppointmentsQuery, PagedResponse<GetClientAppointmentsResponse>>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IUserRepository _userRepository;

    public GetClientAppointmentsHandler(IAppointmentRepository appointmentRepository, IUserRepository userRepository)
    {
        _appointmentRepository = appointmentRepository;
        _userRepository = userRepository;
    }

    public async Task<PagedResponse<GetClientAppointmentsResponse>> Handle(GetClientAppointmentsQuery query, CancellationToken cancellationToken)
    {
        var client = await _userRepository.GetByIdAsync(query.ClientId);
        if (client is null)
            throw new UserNotFoundException();

        var (appointments, totalCount) = await _appointmentRepository.GetByClientIdPagedAsync(
            query.ClientId,
            query.Pagination.Page,
            query.Pagination.PageSize);

        var items = appointments
            .Select(a =>
            {
                var services = a.Services.Select(x =>
                    new AppointmentServiceDto(x.ServiceId, x.ServiceName, x.Price, x.Duration)).ToList();
                
                return new GetClientAppointmentsResponse(
                    a.Id,
                    a.MasterProfileId,
                    a.MasterProfile.Master.GetFullName(),
                    a.MasterProfile.PhotoUrl,
                    services,
                    a.StartTime,
                    a.EndTime,
                    a.Status,
                    a.Comment);
            })
            .ToList();

        return new PagedResponse<GetClientAppointmentsResponse>(items, totalCount, query.Pagination.Page, query.Pagination.PageSize);
    }
}
