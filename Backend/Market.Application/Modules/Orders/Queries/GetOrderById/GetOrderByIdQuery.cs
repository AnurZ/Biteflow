using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Application.Modules.Orders.Queries.GetOrderById
{
    public sealed class GetOrderByIdQuery : IRequest<OrderByIdDto>
    {
        public int Id { get; set; }
    }
}
