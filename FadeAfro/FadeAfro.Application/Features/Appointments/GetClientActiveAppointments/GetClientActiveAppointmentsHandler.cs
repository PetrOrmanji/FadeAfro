using FadeAfro.Application.Features.Appointments.Common;
using FadeAfro.Domain.Repositories;
using MediatR;

namespace FadeAfro.Application.Features.Appointments.GetClientActiveAppointments;

public class GetClientActiveAppointmentsHandler : IRequestHandler<GetClientActiveAppointmentsQuery, GetClientActiveAppointmentsResponse>
{
    private readonly IAppointmentRepository _appointmentRepository;

    public GetClientActiveAppointmentsHandler(IAppointmentRepository appointmentRepository)
    {
        _appointmentRepository = appointmentRepository;
    }

    public async Task<GetClientActiveAppointmentsResponse> Handle(
        GetClientActiveAppointmentsQuery query, CancellationToken cancellationToken)
    {
        var activeAppointments = await _appointmentRepository.GetActiveByClientIdAsync(
            query.ClientId,
            includeServices: true,
            includeMasterInfo: true);
        
        var appointmentDtos = new List<AppointmentDto>();

        foreach (var appointment in activeAppointments)
        {
            var appointmentServiceDtos = appointment.Services.Select(x =>
                new AppointmentServiceDto(x.ServiceId, x.ServiceName, x.Price, x.Duration)).ToList();

            var appointmentMasterDto = new AppointmentMasterDto(
                appointment.MasterProfile.Master.FirstName,
                appointment.MasterProfile.Master.LastName,
                appointment.MasterProfile.PhotoUrl);

            var appointmentDto = new AppointmentDto(
                appointment.Id,
                appointment.StartTime,
                appointment.EndTime,
                appointment.Comment,
                appointmentServiceDtos,
                Master: appointmentMasterDto);
            
            appointmentDtos.Add(appointmentDto);
        }

        return new GetClientActiveAppointmentsResponse(appointmentDtos);
    }
}
