using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Domain.Common.Enums
{
    public enum TableStatus
    {
        Free = 0,
        Occupied = 1,
        Reserved = 2,
        Cleaning = 3,
        OutOfService = 4
    }

}
