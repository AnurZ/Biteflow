namespace Market.Shared.Dtos;

public sealed class ErrorDto
{
    public string Code { get; init; }     // e.g. "internal.error"
    public string Message { get; init; }  // short message
}