using MediatR;

namespace Market.Application.Modules.TableLayout.Commands.UpdateTableLayout
{
    public sealed class UpdateTableLayoutCommandDto : IRequest
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string BackgroundColor { get; set; } = "#ffffff";
        public string? FloorImageUrl { get; set; }
    }
}
