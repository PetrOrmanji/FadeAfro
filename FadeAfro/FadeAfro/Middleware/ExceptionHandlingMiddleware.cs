using System.Net;
using System.Text.Json;
using FadeAfro.Domain.Exceptions;
using FadeAfro.Domain.Exceptions.Appointment;
using FadeAfro.Domain.Exceptions.Auth;
using FadeAfro.Domain.Exceptions.MasterProfile;
using FadeAfro.Domain.Exceptions.MasterSchedule;
using FadeAfro.Domain.Exceptions.MasterService;
using FadeAfro.Domain.Exceptions.MasterUnavailability;
using FadeAfro.Domain.Exceptions.User;

namespace FadeAfro.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex, _environment.IsDevelopment());
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception, bool isDevelopment)
    {
        var (statusCode, message) = exception switch
        {
            UserNotFoundException or
            MasterProfileNotFoundException or
            MasterProfilePhotoNotFoundException or
            MasterServiceNotFoundException or
            MasterScheduleNotFoundException or
            MasterUnavailabilityNotFoundException or
            AppointmentNotFoundException
                => (HttpStatusCode.NotFound, exception.Message),

            InvalidInitDataException
                => (HttpStatusCode.Unauthorized, exception.Message),

            UserAlreadyExistsException or
            MasterProfileAlreadyExistsException or
            UserAlreadyMasterException or
            ServiceFromAnotherMasterException or
            ScheduleOfAnotherMasterException or 
            UnavailabilityOfAnotherMasterException or
            AppointmentOfAnotherClient or
            AppointmentOfAnotherMaster or
            MasterUnavailabilityAlreadyExistsException or
            MasterUnavailabilityConflictException or
            MasterServiceConflictException
                => (HttpStatusCode.Conflict, exception.Message),

            InvalidFirstNameException or
            InvalidTelegramIdException or
            EmptyRolesException or
            InvalidMasterServiceNameException or
            InvalidMasterServicePriceException or
            InvalidMasterServiceDurationException or
            InvalidAppointmentTimeException or
            InvalidAppointmentStatusException or
            InvalidMasterScheduleTimeException
                => (HttpStatusCode.BadRequest, exception.Message),

            DomainException => (HttpStatusCode.BadRequest, exception.Message),

            _ => (HttpStatusCode.InternalServerError, isDevelopment ? exception.Message : "An unexpected error occurred.")
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = JsonSerializer.Serialize(new { error = message });
        await context.Response.WriteAsync(response);
    }
}
