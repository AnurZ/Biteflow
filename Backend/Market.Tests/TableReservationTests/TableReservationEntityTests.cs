using Market.Domain.Common.Enums;
using Market.Domain.Entities.TableReservations;

namespace Market.Tests.TableReservationTests;

public sealed class TableReservationEntityTests
{
    private static readonly DateTime Now = new(2026, 05, 08, 12, 0, 0, DateTimeKind.Utc);

    [Theory]
    [InlineData(ReservationStatus.Confirmed, null, true)]
    [InlineData(ReservationStatus.Confirmed, 1, true)]
    [InlineData(ReservationStatus.Confirmed, -1, false)]
    [InlineData(ReservationStatus.Pending, null, false)]
    [InlineData(ReservationStatus.Pending, 1, false)]
    [InlineData(ReservationStatus.Cancelled, null, false)]
    [InlineData(ReservationStatus.Cancelled, 1, false)]
    public void IsActiveAt_ShouldOnlyTreatConfirmedFutureOrOpenEndedReservationsAsActive(
        ReservationStatus status,
        int? endOffsetHours,
        bool expected)
    {
        var reservation = new TableReservation
        {
            Status = status,
            ReservationStart = Now.AddHours(-2),
            ReservationEnd = endOffsetHours.HasValue ? Now.AddHours(endOffsetHours.Value) : null
        };

        Assert.Equal(expected, reservation.IsActiveAt(Now));
    }
}
