using MediatR;

public sealed class GetMealByIdQuery : IRequest<GetMealByIdDto>
{
    public int Id { get; set; }
}
