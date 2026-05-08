using FluentValidation;
using Market.Application.Common.Exceptions;
using Market.Shared.Dtos;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using DataAnnotationsValidationException = System.ComponentModel.DataAnnotations.ValidationException;

namespace Market.Infrastructure.Common;

/// <summary>
/// Global exception handler for unhandled exceptions.
/// Keeps the same ErrorDto format as the previous middleware.
/// </summary>
public sealed class MarketExceptionHandler(
    ILogger<MarketExceptionHandler> logger,
    IHostEnvironment env
) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext ctx, Exception ex, CancellationToken ct)
    {
        // If the response has already started, let it bubble up
        if (ctx.Response.HasStarted)
        {
            logger.LogWarning(ex, "Response already started, letting the exception bubble.");
            return false;
        }

        var traceId = Activity.Current?.Id ?? ctx.TraceIdentifier;

        logger.LogError(ex,
            "Unhandled exception. Path: {Path}, Method: {Method}, TraceId: {TraceId}, User: {User}",
            ctx.Request.Path,
            ctx.Request.Method,
            traceId,
            ctx.User.Identity?.Name ?? "anonymous");


        ctx.Response.ContentType = "application/json";
        ctx.Response.StatusCode = ex switch
        {
            MarketNotFoundException or KeyNotFoundException => StatusCodes.Status404NotFound,
            MarketConflictException or MarketBusinessRuleException or InvalidOperationException => StatusCodes.Status409Conflict,
            TenantContextMissingException => StatusCodes.Status403Forbidden,
            ValidationException or DataAnnotationsValidationException or ArgumentException => StatusCodes.Status400BadRequest,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status500InternalServerError
        };

        var error = BuildErrorDto(ex, env.IsDevelopment(), traceId);

        await ctx.Response.WriteAsJsonAsync(error, cancellationToken: ct);
        return true; // prevents rethrowing the exception
    }

    private static ErrorDto BuildErrorDto(Exception ex, bool isDev, string traceId)
    {
        string code = "internal.error";
        string message = "An error occurred. Please try again.";

        switch (ex)
        {
            case MarketNotFoundException:
            case KeyNotFoundException:
            case MarketConflictException:
            case MarketBusinessRuleException:
            case InvalidOperationException:
            case TenantContextMissingException:
                code = "entity.error";
                message = ex.Message;
                break;

            case ArgumentException:
            case DataAnnotationsValidationException:
                code = "validation.error";
                message = ex.Message;
                break;

            case UnauthorizedAccessException:
                code = "unauthorized.error";
                message = "Activation link is invalid or expired.";
                break;

            case ValidationException vex:
                code = "validation.error";
                var validationErrors = vex.Errors
                    .Select(e => string.IsNullOrWhiteSpace(e.PropertyName)
                        ? e.ErrorMessage
                        : $"{e.PropertyName}: {e.ErrorMessage}")
                    .Where(e => !string.IsNullOrWhiteSpace(e))
                    .ToArray();

                message = validationErrors.Length > 0
                    ? "Validation failed: " + string.Join("; ", validationErrors)
                    : vex.Message;
                break;
        }

        return new ErrorDto
        {
            Code = code,
            Message = message,
            TraceId = traceId,
            Details = isDev ? ex.ToString() : null // stack trace only in Development environment
        };
    }
}
