using FadeAfro.Application.Features.Appointments.Common;
using FadeAfro.Domain.Exceptions.MasterProfile;
using FadeAfro.Domain.Repositories;
using MediatR;

namespace FadeAfro.Application.Features.Appointments.GetMasterActualAppointments;

public class GetMasterActualAppointmentsHandler : IRequestHandler<GetMasterActualAppointmentsQuery, GetMasterActualAppointmentsResponse>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IMasterProfileRepository _masterProfileRepository;

    public GetMasterActualAppointmentsHandler(IAppointmentRepository appointmentRepository, IMasterProfileRepository masterProfileRepository)
    {
        _appointmentRepository = appointmentRepository;
        _masterProfileRepository = masterProfileRepository;
    }

    public async Task<GetMasterActualAppointmentsResponse> Handle(GetMasterActualAppointmentsQuery query, CancellationToken cancellationToken)
    {
        var masterProfile = await _masterProfileRepository.GetByMasterIdAsync(query.MasterId);
        if (masterProfile is null)
            throw new MasterProfileNotFoundException();

        var actualAppointments = await _appointmentRepository.GetActualByMasterProfileIdAsync(
                masterProfile.Id,
                includeServices: true,
                includeClientInfo: true);

        var appointmentDtos = new List<AppointmentDto>();

        foreach (var appointment in actualAppointments)
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

        return new GetMasterActualAppointmentsResponse(appointmentDtos);
    }
}
