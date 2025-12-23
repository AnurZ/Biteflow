using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace Market.Application.Modules.TableLayout.Queries.TableLayoutGetNameById
{
    

    public sealed record TableLayoutGetNameByIdQuery(int Id)
        : IRequest<TableLayoutGetNameByIdDto>;


    public sealed class TableLayoutGetNameByIdDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
    }

}
