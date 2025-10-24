using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Market.Domain.Common;

namespace Market.Domain.Entities.Identity
{
    public class AppUser : BaseEntity
    {
        public Guid RestaurantId { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public bool IsEmailConfirmed { get; set; }
        public bool IsLocked { get; set; }
        public string? EncryptedSensitiveData { get; set; }
        public int TokenVersion { get; set; } = 0;
        public bool IsEnabled { get; set; }

        public ICollection<RefreshTokenEntity> RefreshTokens { get; set; } = new List<RefreshTokenEntity>();
    }

}
