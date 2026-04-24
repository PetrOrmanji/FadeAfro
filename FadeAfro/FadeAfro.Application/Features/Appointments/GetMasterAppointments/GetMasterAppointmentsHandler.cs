using FadeAfro.Domain.Exceptions.MasterProfile;
using FadeAfro.Domain.Repositories;
using MediatR;

namespace FadeAfro.Application.Features.Appointments.GetMasterAppointments;

public class GetMasterAppointmentsHandler : IRequestHandler<GetMasterAppointmentsQuery, GetMasterAppointmentsResponse>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IMasterProfileRepository _masterProfileRepository;

    public GetMasterAppointmentsHandler(IAppointmentRepository appointmentRepository, IMasterProfileRepository masterProfileRepository)
    {
        _appointmentRepository = appointmentRepository;
        _masterProfileRepository = masterProfileRepository;
    }

    public async Task<GetMasterAppointmentsResponse> Handle(GetMasterAppointmentsQuery query, CancellationToken cancellationToken)
    {
        var masterProfile = await _masterProfileRepository.GetByIdAsync(query.MasterProfileId);

        if (masterProfile is null)
            throw new MasterProfileNotFoundException();

        var appointments = await _appointmentRepository.GetByMasterProfileIdAsync(query.MasterProfileId);

        var response = appointments
            .Select(a => new MasterAppointmentResponse(
                a.Id,
                a.ClientId,
                a.ServiceId,
                a.StartTime,
                a.EndTime,
                a.Status,
                a.Comment))
            .ToList();

        return new GetMasterAppointmentsResponse(response);
    }
}
