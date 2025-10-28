using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Application.Modules.Meal.Commands.Delete
{
    public sealed class DeleteMealCommand:IRequest
    {
        public int Id { get; set; }
    }
}
