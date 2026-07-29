using Bank.Shared.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Bank.Shared;

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        string traceId = httpContext.TraceIdentifier;

        Error error;
        int statusCode;

        if (exception is GeneralException generalException)
        {
            error = generalException.Error;
            statusCode = generalException.StatusCode;

            _logger.LogWarning(
                exception,
                "Handled application exception. ErrorCode: {ErrorCode}, TraceId: {TraceId}",
                error.ErrorCode,
                traceId
            );
        }
        else
        {
            error = Errors.UnexpectedError;
            statusCode = StatusCodes.Status500InternalServerError;

            _logger.LogError(
                exception,
                "An unhandled exception occurred. TraceId: {TraceId}",
                traceId
            );
        }

        var response = new ErrorResponse(
            error
        );

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(
            response,
            cancellationToken
        );

        return true;
    }
    
    
}