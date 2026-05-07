using Market.Domain.Entities.IdentityV2;
using Market.Domain.Entities.TableReservations;
using Market.Domain.Common.Enums;
using Market.Application.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Market.Domain.Common.Enums;

namespace Market.Application.Modules.TableReservation.Commands.CreateTableReservation
{
    public sealed class CreateTableReservationCommandHandler
        : IRequestHandler<CreateTableReservationCommandDto, int>
    {
        private readonly IAppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITenantContext _tenantContext;

        public CreateTableReservationCommandHandler(
            IAppDbContext db,
            UserManager<ApplicationUser> userManager,
            ITenantContext tenantContext)
        {
            _db = db;
            _userManager = userManager;
            _tenantContext = tenantContext;
        }

        public async Task<int> Handle(
            CreateTableReservationCommandDto request,
            CancellationToken cancellationToken)
        {
            var tenantId = _tenantContext.RequireTenantId();

            if (request.NumberOfGuests <= 0)
                throw new ArgumentException("Number of guests must be greater than zero.");

            if (request.ReservationEnd.HasValue &&
                request.ReservationStart >= request.ReservationEnd)
                throw new ArgumentException("Reservation start must be before reservation end.");

            if (request.ApplicationUserId.HasValue)
            {
                var user = await _userManager.FindByIdAsync(request.ApplicationUserId.Value.ToString());

                if (user == null)
                    throw new KeyNotFoundException("User not found.");

                if (user.TenantId != tenantId)
                    throw new InvalidOperationException("User does not belong to this tenant.");
            }

            var table = await _db.DiningTables
                .FirstOrDefaultAsync(t =>
                    t.Id == request.DiningTableId &&
                    t.TenantId == tenantId,
                    cancellationToken);

            if (table == null)
                throw new InvalidOperationException("Dining table not found.");

            if (request.NumberOfGuests > table.NumberOfSeats)
                throw new ArgumentException("Too many guests for this table.");

            var overlapping = await _db.TableReservations
                .AnyAsync(r =>
                     r.DiningTableId == request.DiningTableId &&
                     r.Status != ReservationStatus.Cancelled &&
                     request.ReservationStart < (r.ReservationEnd ?? DateTime.MaxValue) &&
                     r.ReservationStart < (request.ReservationEnd ?? DateTime.MaxValue),
                     cancellationToken);


            if (overlapping)
                throw new InvalidOperationException("Table already reserved in this time slot.");

            var reservation = new Domain.Entities.TableReservations.TableReservation
            {
                DiningTableId = request.DiningTableId,
                TenantId = tenantId,
                NumberOfGuests = request.NumberOfGuests,
                ApplicationUserId = request.ApplicationUserId,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Notes = request.Notes,
                ReservationStart = request.ReservationStart,
                ReservationEnd = request.ReservationEnd,
                Status = ReservationStatus.Pending
            };

            _db.TableReservations.Add(reservation);
            await _db.SaveChangesAsync(cancellationToken);

            return reservation.Id;
        }
    }
}