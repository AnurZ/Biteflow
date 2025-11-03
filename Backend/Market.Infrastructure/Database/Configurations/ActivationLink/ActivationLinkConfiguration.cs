using Market.Domain.Entities.ActivationLinkEntity;

namespace Market.Infrastructure.Database.Configurations.ActivationLink
{
    public sealed class ActivationLinkConfiguration : IEntityTypeConfiguration<ActivationLinkEntity>
    {
        public void Configure(EntityTypeBuilder<ActivationLinkEntity> b)
        {
            b.ToTable("ActivationLinks");
            b.HasKey(x => x.Id);

            b.Property(x => x.TokenHash)
                .IsRequired()
                .HasMaxLength(128); 

            b.HasIndex(x => x.TokenHash).IsUnique();

            b.HasIndex(x => new { x.RequestId, x.ExpiresAtUtc, x.ConsumedAtUtc });

            // One active link per request

            b.Property(x => x.ExpiresAtUtc).IsRequired();
        }
    }
}
