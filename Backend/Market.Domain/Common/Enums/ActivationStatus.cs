using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Domain.Common.Enums
{
    public enum ActivationStatus
    {
        Draft= 0, // User is filling the wizzard
        Submitted = 1, // Awaiting admin review
        Approved = 2, // Admin approved
        Rejected = 3, // Admin rejected
        Activated = 4 // tenant finished activation (clicked link)
    }
}
