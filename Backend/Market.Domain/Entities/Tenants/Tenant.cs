namespace Market.Domain.Entities.Tenants;

public sealed class Tenant
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int? ActivationRequestId { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<Restaurant> Restaurants { get; set; } = new List<Restaurant>();
}
