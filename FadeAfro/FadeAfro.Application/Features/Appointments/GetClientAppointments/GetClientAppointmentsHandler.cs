using FadeAfro.Domain.Exceptions.User;
using FadeAfro.Domain.Repositories;
using MediatR;

namespace FadeAfro.Application.Features.Appointments.GetClientAppointments;

public class GetClientAppointmentsHandler : IRequestHandler<GetClientAppointmentsQuery, GetClientAppointmentsResponse>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IUserRepository _userRepository;

    public GetClientAppointmentsHandler(IAppointmentRepository appointmentRepository, IUserRepository userRepository)
    {
        _appointmentRepository = appointmentRepository;
        _userRepository = userRepository;
    }

    public async Task<GetClientAppointmentsResponse> Handle(GetClientAppointmentsQuery query, CancellationToken cancellationToken)
    {
        var client = await _userRepository.GetByIdAsync(query.ClientId);

        if (client is null)
            throw new UserNotFoundException();

        var appointments = await _appointmentRepository.GetByClientIdAsync(query.ClientId);

        var response = appointments
            .Select(a => new AppointmentResponse(
                a.Id,
                a.MasterProfileId,
                a.ServiceId,
                a.StartTime,
                a.EndTime,
                a.Status,
                a.Comment))
            .ToList();

        return new GetClientAppointmentsResponse(response);
    }
}
