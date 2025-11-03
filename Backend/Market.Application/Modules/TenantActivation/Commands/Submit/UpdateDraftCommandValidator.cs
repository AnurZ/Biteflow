using Market.Application.Modules.TenantActivation.Commands.Update;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Application.Modules.TenantActivation.Commands.Submit
{
    public sealed class UpdateDraftCommandValidator : AbstractValidator<UpdateDraftCommand>
    {
        public UpdateDraftCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.RestaurantName).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Domain).NotEmpty().MaximumLength(120).Matches("^[a-z0-9-\\.]+$");
            RuleFor(x => x.OwnerFullName).NotEmpty().MaximumLength(120);
            RuleFor(x => x.OwnerEmail).NotEmpty().EmailAddress();
            RuleFor(x => x.OwnerPhone).NotEmpty().MaximumLength(40);
            RuleFor(x => x.Address).NotEmpty().MaximumLength(200);
            RuleFor(x => x.City).NotEmpty().MaximumLength(80);
            RuleFor(x => x.State).NotEmpty().MaximumLength(80);
        }
    }
}
