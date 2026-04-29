using FadeAfro.Application.Features.Appointments.Common;
using FadeAfro.Domain.Repositories;
using MediatR;

namespace FadeAfro.Application.Features.Appointments.GetClientActualAppointments;

public class GetClientActualAppointmentsHandler : IRequestHandler<GetClientActualAppointmentsQuery, GetClientActualAppointmentsResponse>
{
    private readonly IAppointmentRepository _appointmentRepository;

    public GetClientActualAppointmentsHandler(IAppointmentRepository appointmentRepository)
    {
        _appointmentRepository = appointmentRepository;
    }

    public async Task<GetClientActualAppointmentsResponse> Handle(
        GetClientActualAppointmentsQuery query, CancellationToken cancellationToken)
    {
        var actualAppointments = await _appointmentRepository.GetActualByClientIdAsync(
            query.ClientId,
            includeServices: true,
            includeMasterInfo: true);
        
        var appointmentDtos = new List<AppointmentDto>();

        foreach (var appointment in actualAppointments)
        {
            var appointmentServiceDtos = appointment.Services.Select(x =>
                new AppointmentServiceDto(x.ServiceId, x.ServiceName, x.Price, x.Duration)).ToList();

            var appointmentMasterDto = new AppointmentMasterDto(
                appointment.MasterProfile.Master.FirstName,
                appointment.MasterProfile.Master.LastName,
                appointment.MasterProfile.Description,
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

        return new GetClientActualAppointmentsResponse(appointmentDtos);
    }
}
