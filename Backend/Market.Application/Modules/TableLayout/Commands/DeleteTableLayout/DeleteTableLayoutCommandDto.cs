using MediatR;

namespace Market.Application.Modules.TableLayout.Commands.DeleteTableLayout
{
    public sealed class DeleteTableLayoutCommandDto : IRequest
    {
        public int Id { get; set; }
    }
}
