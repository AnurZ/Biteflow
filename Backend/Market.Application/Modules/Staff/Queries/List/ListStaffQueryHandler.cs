using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Application.Modules.Staff.Queries.List
{
    public sealed class ListStaffQueryHandler(IAppDbContext db)
    : IRequestHandler<ListStaffQuery, PageResult<ListStaffItemDto>>
    {
        public async Task<PageResult<ListStaffItemDto>> Handle(ListStaffQuery req, CancellationToken ct)
        {
            var q = db.EmployeeProfiles
                .AsNoTracking()
                .Include(e => e.AppUser)
                .Select(e => new ListStaffItemDto
                {
                    Id = e.Id,
                    AppUserId = e.AppUserId,
                    DisplayName = e.AppUser.DisplayName,
                    Email = e.AppUser.Email,
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    Position = e.Position,
                    IsActive = e.IsActive,
                    HireDate = e.HireDate
                });

            if (!string.IsNullOrWhiteSpace(req.Search))
            {
                var s = req.Search.Trim().ToLower();
                q = q.Where(x =>
                    x.FirstName.ToLower().Contains(s) ||
                    x.LastName.ToLower().Contains(s) ||
                    x.DisplayName.ToLower().Contains(s) ||
                    x.Email.ToLower().Contains(s) ||
                    x.Position.ToLower().Contains(s));
            }

            // Simple sort parser: "-hireDate" means desc
            if (!string.IsNullOrWhiteSpace(req.Sort))
            {
                bool desc = req.Sort.StartsWith("-");
                string key = desc ? req.Sort[1..] : req.Sort;

                q = key switch
                {
                    "firstName" => desc ? q.OrderByDescending(x => x.FirstName) : q.OrderBy(x => x.FirstName),
                    "lastName" => desc ? q.OrderByDescending(x => x.LastName) : q.OrderBy(x => x.LastName),
                    "hireDate" => desc ? q.OrderByDescending(x => x.HireDate) : q.OrderBy(x => x.HireDate),
                    "position" => desc ? q.OrderByDescending(x => x.Position) : q.OrderBy(x => x.Position),
                    _ => q.OrderBy(x => x.Id)
                };
            }
            else
            {
                q = q.OrderBy(x => x.Id);
            }

            return await PageResult<ListStaffItemDto>.FromQueryableAsync(q, req.Paging, ct);

        }
    }
}
