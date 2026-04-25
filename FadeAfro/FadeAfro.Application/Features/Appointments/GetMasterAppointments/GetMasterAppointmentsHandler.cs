using FadeAfro.Application.Common;
using FadeAfro.Domain.Exceptions.MasterProfile;
using FadeAfro.Domain.Repositories;
using MediatR;

namespace FadeAfro.Application.Features.Appointments.GetMasterAppointments;

public class GetMasterAppointmentsHandler : IRequestHandler<GetMasterAppointmentsQuery, PagedResponse<MasterAppointmentResponse>>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IMasterProfileRepository _masterProfileRepository;

    public GetMasterAppointmentsHandler(IAppointmentRepository appointmentRepository, IMasterProfileRepository masterProfileRepository)
    {
        _appointmentRepository = appointmentRepository;
        _masterProfileRepository = masterProfileRepository;
    }

    public async Task<PagedResponse<MasterAppointmentResponse>> Handle(GetMasterAppointmentsQuery query, CancellationToken cancellationToken)
    {
        var masterProfile = await _masterProfileRepository.GetByIdAsync(query.MasterProfileId);

        if (masterProfile is null)
            throw new MasterProfileNotFoundException();

        var (appointments, totalCount) = await _appointmentRepository.GetByMasterProfileIdPagedAsync(
            query.MasterProfileId,
            query.Pagination.Page,
            query.Pagination.PageSize);

        var items = appointments
            .Select(a => new MasterAppointmentResponse(
                a.Id,
                a.ClientId,
                a.ServiceId,
                a.StartTime,
                a.EndTime,
                a.Status,
                a.Comment))
            .ToList();

        return new PagedResponse<MasterAppointmentResponse>(items, totalCount, query.Pagination.Page, query.Pagination.PageSize);
    }
}
