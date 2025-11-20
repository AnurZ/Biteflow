using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Application.Modules.MealCategory.Commands.DeleteMealCategoryCommand
{
    public sealed class DeleteMealCategoryCommandDto:IRequest
    {
        public int Id { get; set; }
    }
}
