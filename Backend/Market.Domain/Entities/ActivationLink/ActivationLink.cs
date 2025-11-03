using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Domain.Entities.ActivationLinkEntity
{
    public sealed class ActivationLinkEntity
    {
        public int Id { get; set; }

        public int RequestId { get; set; }
        public string TokenHash { get; set; } = string.Empty;

        public DateTimeOffset ExpiresAtUtc { get; set; }
        public DateTimeOffset? ConsumedAtUtc { get; set; }

        public string? IssuedBy { get; set; }
        public string? ConsumedBy { get; set; }
    }
}
