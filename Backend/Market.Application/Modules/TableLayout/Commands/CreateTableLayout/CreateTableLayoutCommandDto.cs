using MediatR;
using System.Collections.Generic;

namespace Market.Application.Modules.TableLayout.Commands.CreateTableLayout
{
    public sealed class CreateTableLayoutCommandDto : IRequest<int>
    {
        public string Name { get; set; } = string.Empty;
        public string BackgroundColor { get; set; } = "#ffffff";
        public string? FloorImageUrl { get; set; }
    }
}
