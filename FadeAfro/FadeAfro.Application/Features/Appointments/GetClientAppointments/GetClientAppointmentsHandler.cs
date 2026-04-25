using FadeAfro.Application.Common;
using FadeAfro.Domain.Exceptions.User;
using FadeAfro.Domain.Repositories;
using MediatR;

namespace FadeAfro.Application.Features.Appointments.GetClientAppointments;

public class GetClientAppointmentsHandler : IRequestHandler<GetClientAppointmentsQuery, PagedResponse<AppointmentResponse>>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IUserRepository _userRepository;

    public GetClientAppointmentsHandler(IAppointmentRepository appointmentRepository, IUserRepository userRepository)
    {
        _appointmentRepository = appointmentRepository;
        _userRepository = userRepository;
    }

    public async Task<PagedResponse<AppointmentResponse>> Handle(GetClientAppointmentsQuery query, CancellationToken cancellationToken)
    {
        var client = await _userRepository.GetByIdAsync(query.ClientId);

        if (client is null)
            throw new UserNotFoundException();

        var (appointments, totalCount) = await _appointmentRepository.GetByClientIdPagedAsync(
            query.ClientId,
            query.Pagination.Page,
            query.Pagination.PageSize);

        var items = appointments
            .Select(a => new AppointmentResponse(
                a.Id,
                a.MasterProfileId,
                a.ServiceId,
                a.StartTime,
                a.EndTime,
                a.Status,
                a.Comment))
            .ToList();

        return new PagedResponse<AppointmentResponse>(items, totalCount, query.Pagination.Page, query.Pagination.PageSize);
    }
}
