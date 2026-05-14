using Market.Domain.Common;
using Market.Domain.Entities.IdentityV2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Domain.Entities.DashboardLayout
{
    public class DashboardLayout : BaseEntity
    {
        public int Id { get; set; }
        public Guid ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; } = null;
        public string LayoutJson { get; set; } = string.Empty;
    }
}
