using Market.Application.Common.Exceptions;

public sealed class ExceptionMiddleware(
    ILogger<ExceptionMiddleware> logger,
    IHostEnvironment env) : IMiddleware
{
    public async Task InvokeAsync(HttpContext ctx, RequestDelegate next)
    {
        try
        {
            await next(ctx);
        }
        catch (Exception ex)
        {
            if (ctx.Response.HasStarted)
            {
                logger.LogWarning(ex, "Response already started, rethrowing");
                throw;
            }

            logger.LogError(ex, "Unhandled exception");

            ctx.Response.ContentType = "application/json";
            ctx.Response.StatusCode = ex switch
            {
                MarketNotFoundException => StatusCodes.Status404NotFound,
                MarketConflictException => StatusCodes.Status409Conflict,
                MarketBusinessRuleException => StatusCodes.Status409Conflict,
                ValidationException => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status500InternalServerError
            };

            var traceId = System.Diagnostics.Activity.Current?.Id ?? ctx.TraceIdentifier;

            var error = BuildErrorDto(ex, env.IsDevelopment(), traceId);

            await ctx.Response.WriteAsJsonAsync(error);
        }
    }

    private static ErrorDto BuildErrorDto(Exception ex, bool isDev, string traceId)
    {
        string code = "internal.error";
        string message = "An error occurred. Please try again.";

        List<FieldErrorDto>? fieldErrors = null;

        switch (ex)
        {
            case MarketNotFoundException:
            case MarketConflictException:
            case MarketBusinessRuleException:
                code = "entity.error";
                message = ex.Message;
                break;

            case ValidationException vex:
                code = "validation.error";
                message = "Validation failed.";
                fieldErrors = vex.Errors
                    .Select(e => new FieldErrorDto
                    {
                        Field = e.PropertyName,
                        Message = e.ErrorMessage,
                        ErrorCode = e.ErrorCode
                    })
                    .ToList();
                break;
        }

        return new ErrorDto
        {
            Code = code,
            Message = message,
            TraceId = traceId,
            Errors = fieldErrors,
            // DEV environment can get details:
            Details = isDev ? ex.ToString() : null
        };
    }
}

public sealed class ErrorDto
{
    public string Code { get; set; } = default!;
    public string Message { get; set; } = default!;
    public string? TraceId { get; set; }
    public string? Details { get; set; }           // only in Dev
    public List<FieldErrorDto>? Errors { get; set; } // per-field validations
}

public sealed class FieldErrorDto
{
    public string Field { get; set; } = default!;
    public string Message { get; set; } = default!;
    public string? ErrorCode { get; set; }
}