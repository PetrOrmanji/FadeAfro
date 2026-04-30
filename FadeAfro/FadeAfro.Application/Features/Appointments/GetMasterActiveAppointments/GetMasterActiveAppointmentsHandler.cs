using FadeAfro.Application.Features.Appointments.Common;
using FadeAfro.Domain.Exceptions.MasterProfile;
using FadeAfro.Domain.Repositories;
using MediatR;

namespace FadeAfro.Application.Features.Appointments.GetMasterActiveAppointments;

public class GetMasterActiveAppointmentsHandler : IRequestHandler<GetMasterActiveAppointmentsQuery, GetMasterActiveAppointmentsResponse>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IMasterProfileRepository _masterProfileRepository;

    public GetMasterActiveAppointmentsHandler(IAppointmentRepository appointmentRepository, IMasterProfileRepository masterProfileRepository)
    {
        _appointmentRepository = appointmentRepository;
        _masterProfileRepository = masterProfileRepository;
    }

    public async Task<GetMasterActiveAppointmentsResponse> Handle(GetMasterActiveAppointmentsQuery query, CancellationToken cancellationToken)
    {
        var masterProfile = await _masterProfileRepository.GetByMasterIdAsync(query.MasterId);
        if (masterProfile is null)
            throw new MasterProfileNotFoundException();

        var activeAppointments = await _appointmentRepository.GetActiveByMasterProfileIdAsync(
                masterProfile.Id,
                includeServices: true,
                includeClientInfo: true);

        var appointmentDtos = new List<AppointmentDto>();

        foreach (var appointment in activeAppointments)
        {
            var appointmentServiceDtos = appointment.Services.Select(x =>
                new AppointmentServiceDto(x.ServiceId, x.ServiceName, x.Price, x.Duration)).ToList();

            var appointmentClientDto = new AppointmentClientDto(
                appointment.Client.FirstName,
                appointment.Client.LastName,
                appointment.Client.Username);
            
            var appointmentDto = new AppointmentDto(
                appointment.Id,
                appointment.StartTime,
                appointment.EndTime,
                appointment.Comment,
                appointmentServiceDtos,
                Client: appointmentClientDto);
            
            appointmentDtos.Add(appointmentDto);
        }

        return new GetMasterActiveAppointmentsResponse(appointmentDtos);
    }
}
