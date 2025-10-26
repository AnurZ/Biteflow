// Market.Application.Modules.Staff.Commands.Create.CreateStaffCommandHandler
using Market.Domain.Entities.Identity;
using Market.Domain.Entities.Staff;
using MediatR;
using Microsoft.AspNetCore.Identity;      
using Microsoft.EntityFrameworkCore;

namespace Market.Application.Modules.Staff.Commands.Create
{
    public sealed class CreateStaffCommandHandler(IAppDbContext db, IPasswordHasher<AppUser> hasher)
        : IRequestHandler<CreateStaffCommand, int>
    {
        public async Task<int> Handle(CreateStaffCommand r, CancellationToken ct)
        {
            
            if (string.IsNullOrWhiteSpace(r.FirstName) || string.IsNullOrWhiteSpace(r.LastName))
                throw new ValidationException("FirstName and LastName are required.");

            int appUserId;

            //If user exsist then set the id
            if (r.AppUserId > 0)
            {
                
                var exists = await db.Users.AnyAsync(u => u.Id == r.AppUserId, ct);
                if (!exists) throw new ValidationException("AppUserId is invalid.");
                appUserId = r.AppUserId;
            }
            else
            {
                // Creating a new user
                if (string.IsNullOrWhiteSpace(r.Email))
                    throw new ValidationException("Email is required when creating a new user.");

                var email = r.Email.Trim();

                var emailTaken = await db.Users.AnyAsync(u => u.Email == email, ct);
                if (emailTaken) throw new MarketConflictException("Email already in use.");

                var displayName = string.IsNullOrWhiteSpace(r.DisplayName)
                    ? $"{r.FirstName} {r.LastName}".Trim()
                    : r.DisplayName!.Trim();

                // Generate a one time only password
                var plainPassword = string.IsNullOrWhiteSpace(r.PlainPassword)
                    ? Guid.NewGuid().ToString("N")
                    : r.PlainPassword!;

                
                var user = new AppUser
                {
                    // TODO: set RestaurantId from TenantCOntext
                    RestaurantId = Guid.Empty,
                    Email = email,
                    DisplayName = displayName,
                    IsEmailConfirmed = false,
                    IsLocked = false,
                    IsEnabled = true,
                    TokenVersion = 0,
                };

                user.PasswordHash = hasher.HashPassword(user, plainPassword);

                db.Users.Add(user);
                await db.SaveChangesAsync(ct); // ensures user.Id

                appUserId = user.Id;
            }

            var profile = new EmployeeProfile
            {
                AppUserId = appUserId,
                Position = r.Position.Trim(),
                FirstName = r.FirstName.Trim(),
                LastName = r.LastName.Trim(),
                PhoneNumber = r.PhoneNumber,
                HireDate = r.HireDate,
                HourlyRate = r.HourlyRate,
                EmploymentType = r.EmploymentType,
                ShiftType = r.ShiftType,
                ShiftStart = r.ShiftStart,
                ShiftEnd = r.ShiftEnd,
                IsActive = r.IsActive,
                Notes = r.Notes
            };

            db.EmployeeProfiles.Add(profile);
            await db.SaveChangesAsync(ct);

            return profile.Id;
        }
    }
}
