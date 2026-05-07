namespace Market.Application.Abstractions;

public interface IStaffIdentityTerminationService
{
    Task TerminateAsync(Guid applicationUserId, CancellationToken ct);
}
