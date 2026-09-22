using Metrica.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Metrica.Api.ExceptionHandling
{
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
            var traceId = httpContext.TraceIdentifier;

            if (exception is FileLoadPeriodConflictException)
            {
                _logger.LogWarning(
                    exception,
                    "Intento de carga para un periodo bloqueado en {Method} {Path}. " +
                    "TraceId: {TraceId}",
                    httpContext.Request.Method,
                    httpContext.Request.Path,
                    traceId);
            }
            else
            {
                _logger.LogError(
                    exception,
                    "Error no controlado en {Method} {Path}. TraceId: {TraceId}",
                    httpContext.Request.Method,
                    httpContext.Request.Path,
                    traceId);
            }

            var problem = exception switch
            {
                FileLoadPeriodConflictException conflictException =>
                    new ProblemDetails
                    {
                        Status = StatusCodes.Status409Conflict,
                        Title = "El periodo no está disponible.",
                        Detail = conflictException.Message,
                        Instance = httpContext.Request.Path.Value
                    },

                _ => new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Ocurrió un error interno.",
                    Detail = "No se pudo completar la solicitud.",
                    Instance = httpContext.Request.Path.Value
                }
            };

            problem.Extensions["traceId"] = traceId;

            httpContext.Response.StatusCode =
                problem.Status ?? StatusCodes.Status500InternalServerError;

            await httpContext.Response.WriteAsJsonAsync(
                problem,
                options: null,
                contentType: "application/problem+json",
                cancellationToken: cancellationToken);

            return true;
        }
    }
}