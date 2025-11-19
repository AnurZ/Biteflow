using Market.Domain.Common;
using Market.Domain.Entities.Meal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Domain.Entities.MealCategory
{
    public sealed class MealCategory : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
