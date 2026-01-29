using Market.Domain.Common;

namespace Market.Domain.Entities.Notifications
{
    public sealed class NotificationEntity : BaseEntity
    {
        public string? TargetUserId { get; set; }
        public string? TargetRole { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? Link { get; set; }
        public DateTime? ReadAtUtc { get; set; }
    }
}
