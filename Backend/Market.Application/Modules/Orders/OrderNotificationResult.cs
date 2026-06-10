namespace Market.Application.Modules.Orders;

public sealed record OrderNotificationResult(
    int Id,
    string? TargetRole,
    string Title,
    string Message,
    string Type,
    string? Link,
    DateTime CreatedAtUtc,
    DateTime? ReadAtUtc);
