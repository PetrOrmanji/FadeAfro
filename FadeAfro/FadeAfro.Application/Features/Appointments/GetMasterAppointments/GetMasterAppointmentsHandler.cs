using FadeAfro.Application.Common;
using FadeAfro.Application.Common.Appointments;
using FadeAfro.Domain.Exceptions.MasterProfile;
using FadeAfro.Domain.Repositories;
using MediatR;

namespace FadeAfro.Application.Features.Appointments.GetMasterAppointments;

public class GetMasterAppointmentsHandler : IRequestHandler<GetMasterAppointmentsQuery, PagedResponse<GetMasterAppointmentsResponse>>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IMasterProfileRepository _masterProfileRepository;

    public GetMasterAppointmentsHandler(IAppointmentRepository appointmentRepository, IMasterProfileRepository masterProfileRepository)
    {
        _appointmentRepository = appointmentRepository;
        _masterProfileRepository = masterProfileRepository;
    }

    public async Task<PagedResponse<GetMasterAppointmentsResponse>> Handle(GetMasterAppointmentsQuery query, CancellationToken cancellationToken)
    {
        var masterProfile = await _masterProfileRepository.GetByIdAsync(query.MasterProfileId);
        if (masterProfile is null)
            throw new MasterProfileNotFoundException();

        var (appointments, totalCount) = await _appointmentRepository.GetByMasterProfileIdPagedAsync(
            query.MasterProfileId,
            query.Pagination.Page,
            query.Pagination.PageSize);

        var items = appointments
            .Select(a =>
            {
                var services = a.Services.Select(s =>
                    new AppointmentServiceDto(s.ServiceId, s.ServiceName, s.Price, s.Duration)).ToList();
                
                return new GetMasterAppointmentsResponse(
                    a.Id,
                    a.ClientId,
                    services,
                    a.StartTime,
                    a.EndTime,
                    a.Status,
                    a.Comment);
            })
            .ToList();

        return new PagedResponse<GetMasterAppointmentsResponse>(items, totalCount, query.Pagination.Page, query.Pagination.PageSize);
    }
}
