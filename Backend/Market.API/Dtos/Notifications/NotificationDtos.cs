namespace Market.API.Dtos.Notifications
{
    public sealed class NotificationItemDto
    {
        public int Id { get; init; }
        public string Title { get; init; } = string.Empty;
        public string Message { get; init; } = string.Empty;
        public string Type { get; init; } = string.Empty;
        public string? Link { get; init; }
        public DateTime CreatedAtUtc { get; init; }
        public DateTime? ReadAtUtc { get; init; }
        public bool IsRead => ReadAtUtc.HasValue;
    }

    public sealed class NotificationListResponse
    {
        public int TotalCount { get; init; }
        public int UnreadCount { get; init; }
        public IReadOnlyList<NotificationItemDto> Items { get; init; } = Array.Empty<NotificationItemDto>();
    }
}
