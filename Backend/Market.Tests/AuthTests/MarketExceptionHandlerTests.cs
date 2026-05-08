using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;
using FluentValidation.Results;
using Market.Infrastructure.Common;
using Market.Shared.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;

namespace Market.Tests.AuthTests;

public sealed class MarketExceptionHandlerTests
{
    public static TheoryData<Exception, HttpStatusCode, string, string> MappedExceptions => new()
    {
        { new KeyNotFoundException("Missing resource."), HttpStatusCode.NotFound, "entity.error", "Missing resource." },
        { new ArgumentException("Invalid input."), HttpStatusCode.BadRequest, "validation.error", "Invalid input." },
        { new InvalidOperationException("Business rule failed."), HttpStatusCode.Conflict, "entity.error", "Business rule failed." },
        { new ValidationException("Data annotations validation failed."), HttpStatusCode.BadRequest, "validation.error", "Data annotations validation failed." },
        { new UnauthorizedAccessException("Invalid token."), HttpStatusCode.Unauthorized, "unauthorized.error", "Activation link is invalid or expired." },
        { new Exception("Sensitive detail."), HttpStatusCode.InternalServerError, "internal.error", "An error occurred. Please try again." }
    };

    [Theory]
    [MemberData(nameof(MappedExceptions))]
    public async Task TryHandleAsync_MapsKnownExceptionTypes(
        Exception exception,
        HttpStatusCode expectedStatus,
        string expectedCode,
        string expectedMessage)
    {
        var (context, handler) = CreateHandler();

        var handled = await handler.TryHandleAsync(context, exception, CancellationToken.None);

        Assert.True(handled);
        Assert.Equal((int)expectedStatus, context.Response.StatusCode);

        var error = await ReadErrorAsync(context);
        Assert.Equal(expectedCode, error.Code);
        Assert.Equal(expectedMessage, error.Message);
        Assert.Equal(context.TraceIdentifier, error.TraceId);
        Assert.Null(error.Details);
    }

    [Fact]
    public async Task TryHandleAsync_FormatsFluentValidationErrors()
    {
        var (context, handler) = CreateHandler();
        var exception = new FluentValidation.ValidationException(new[]
        {
            new ValidationFailure("Name", "Name is required."),
            new ValidationFailure(string.Empty, "General validation failure.")
        });

        var handled = await handler.TryHandleAsync(context, exception, CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);

        var error = await ReadErrorAsync(context);
        Assert.Equal("validation.error", error.Code);
        Assert.Equal("Validation failed: Name: Name is required.; General validation failure.", error.Message);
        Assert.Null(error.Details);
    }

    private static (DefaultHttpContext Context, MarketExceptionHandler Handler) CreateHandler()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var handler = new MarketExceptionHandler(
            NullLogger<MarketExceptionHandler>.Instance,
            new TestHostEnvironment());

        return (context, handler);
    }

    private static async Task<ErrorDto> ReadErrorAsync(HttpContext context)
    {
        context.Response.Body.Position = 0;
        var error = await JsonSerializer.DeserializeAsync<ErrorDto>(
            context.Response.Body,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return error ?? throw new InvalidOperationException("Expected an error response body.");
    }

    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Production;
        public string ApplicationName { get; set; } = "Market.Tests";
        public string ContentRootPath { get; set; } = Directory.GetCurrentDirectory();
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
