using Market.Application.Features.DashboardLayouts.Commands.SaveDashboardLayout;
using Market.Application.Features.DashboardLayouts.Queries.GetDashboardLayout;
using Market.Domain.Entities.IdentityV2;
using Market.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Market.Api.Controllers
{
    [ApiController]
    [Route("api/dashboard-layout")]
    [Authorize]
    public class DashboardLayoutController : ControllerBase
    {
        private readonly IMediator _mediator;

        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardLayoutController(
            IMediator mediator,
            UserManager<ApplicationUser> userManager)
        {
            _mediator = mediator;
            _userManager = userManager;
        }

        // =========================
        // GET
        // =========================
        [HttpGet]
        [Authorize(Policy = PolicyNames.RestaurantAdmin)]
        public async Task<IActionResult> Get()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Unauthorized();

            var result = await _mediator.Send(
                new GetDashboardLayoutQuery
                {
                    ApplicationUserId = user.Id
                });

            return Ok(result);
        }

        // =========================
        // SAVE
        // =========================
        [HttpPost]
        [Authorize(Policy = PolicyNames.RestaurantAdmin)]
        public async Task<IActionResult> Save(
            SaveDashboardLayoutCommand command)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Unauthorized();

            command.ApplicationUserId = user.Id;

            await _mediator.Send(command);

            return Ok();
        }
    }
}