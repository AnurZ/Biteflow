using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Application.Modules.TenantActivation.Commands.Create
{
    public sealed class CreateOrUpdateCommandValidator : AbstractValidator<CreateDraftCommand>
    {
        public CreateOrUpdateCommandValidator()
        {
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
