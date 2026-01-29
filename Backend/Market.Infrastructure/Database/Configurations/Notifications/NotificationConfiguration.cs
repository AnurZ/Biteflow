using Market.Domain.Entities.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Market.Infrastructure.Database.Configurations.Notifications
{
    public sealed class NotificationConfiguration : IEntityTypeConfiguration<NotificationEntity>
    {
        public void Configure(EntityTypeBuilder<NotificationEntity> builder)
        {
            builder.ToTable("Notifications");

            builder.Property(x => x.TargetUserId)
                .HasMaxLength(64);

            builder.Property(x => x.TargetRole)
                .HasMaxLength(64);

            builder.Property(x => x.Title)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.Message)
                .HasMaxLength(1000)
                .IsRequired();

            builder.Property(x => x.Type)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.Link)
                .HasMaxLength(256);

            builder.HasIndex(x => new { x.TenantId, x.TargetUserId });
            builder.HasIndex(x => new { x.TenantId, x.TargetRole });
        }
    }
}
