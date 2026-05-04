using System.Diagnostics;
using Serilog.Context;

namespace FadeAfro.Middleware;

public class RequestLoggingMiddleware
{
    private const string RequestIdLogPropertyName = "RequestId";
    private const int RequestBodySizeLimit = 32 * 1024;
    private const int ResponseBodySizeLimit = 32 * 1024;

    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        using var disposableHandler =
            LogContext.PushProperty(RequestIdLogPropertyName, context.TraceIdentifier);

        var requestMethodLogString = context.Request.Method;
        var requestPathLogString = context.Request.Path.HasValue ? context.Request.Path.Value : string.Empty;
        var requestBodyLogString = await GetRequestBodyLogStringAsync(context);
        var requestDurationMs = await ProcessRequestAndGetDurationAsync(context);
        var responseStatusCode = context.Response.StatusCode;
        var responseBodyLogString = await GetResponseBodyLogStringAsync(context);

        _logger.LogInformation(
            "HTTP {Method} {Path} → {StatusCode} in {ElapsedMs}ms | Req: {RequestBody} | Res: {ResponseBody}",
            requestMethodLogString, requestPathLogString, responseStatusCode, requestDurationMs,
            requestBodyLogString, responseBodyLogString);
    }

    private async Task<long> ProcessRequestAndGetDurationAsync(HttpContext context)
    {
        var stopWatch = Stopwatch.StartNew();

        await _next(context);

        stopWatch.Stop();

        return stopWatch.ElapsedMilliseconds;
    }

    private static async Task<string> GetRequestBodyLogStringAsync(HttpContext context)
    {
        var isFileUpload =
            context.Request.ContentType?.StartsWith("multipart/", StringComparison.OrdinalIgnoreCase) == true;

        var requestBody = isFileUpload
            ? "(file upload — skipped)"
            : await ReadRequestBodyAsync(context.Request);

        var requestBodyLogString = string.IsNullOrWhiteSpace(requestBody)
            ? string.Empty
            : $"RequestBody: {requestBody}";

        return requestBodyLogString;
    }

    private static async Task<string> GetResponseBodyLogStringAsync(HttpContext context)
    {
        var isFileDownload =
            context.Response.ContentType?.StartsWith("multipart/", StringComparison.OrdinalIgnoreCase) == true;

        var responseBody = isFileDownload
            ? "(file download — skipped)"
            : await ReadResponseBodyAsync(context.Response);

        var responseBodyLogString = string.IsNullOrWhiteSpace(responseBody)
            ? string.Empty
            : $"ResponseBody: {responseBody}";

        return responseBodyLogString;
    }

    private static async Task<string> ReadRequestBodyAsync(HttpRequest request)
    {
        request.EnableBuffering();

        if (request.ContentLength is 0 || !request.Body.CanRead)
            return string.Empty;

        request.Body.Position = 0;

        using var reader = new StreamReader(
            request.Body,
            leaveOpen: true,
            detectEncodingFromByteOrderMarks: false,
            bufferSize: 4096);

        var buffer = new char[RequestBodySizeLimit];
        var read = await reader.ReadAsync(buffer, 0, buffer.Length);
        var body = new string(buffer, 0, read);

        if (read == RequestBodySizeLimit)
            body += "… (truncated)";

        request.Body.Position = 0;

        return body;
    }

    private static async Task<string> ReadResponseBodyAsync(HttpResponse response)
    {
        if (!response.Body.CanRead)
            return string.Empty;

        response.Body.Position = 0;

        using var reader = new StreamReader(
            response.Body,
            leaveOpen: true,
            detectEncodingFromByteOrderMarks: false,
            bufferSize: 4096);

        var buffer = new char[ResponseBodySizeLimit];
        var read = await reader.ReadAsync(buffer, 0, buffer.Length);
        var body = new string(buffer, 0, read);

        if (read == ResponseBodySizeLimit)
            body += "… (truncated)";

        response.Body.Position = 0;

        return body;
    }
}
