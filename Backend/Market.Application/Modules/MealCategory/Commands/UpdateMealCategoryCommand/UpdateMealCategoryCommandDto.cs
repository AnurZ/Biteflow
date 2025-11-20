using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Application.Modules.MealCategory.Commands.UpdateMealCategoryCommand
{
    public sealed class UpdateMealCategoryCommandDto:IRequest
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
