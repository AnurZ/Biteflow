using System.ComponentModel.DataAnnotations;
using Duende.IdentityServer.Services;
using Market.Domain.Entities.IdentityV2;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[AllowAnonymous]
[Route("account")]
public sealed class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IIdentityServerInteractionService _interaction;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        IIdentityServerInteractionService interaction,
        ILogger<AccountController> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _interaction = interaction;
        _logger = logger;
    }

    [HttpGet("login")]
    public IActionResult Login([FromQuery] string? returnUrl)
    {
        var vm = new LoginInputModel
        {
            ReturnUrl = returnUrl ?? string.Empty,
            RememberLogin = true
        };

        return View("Login", vm);
    }

    [HttpPost("login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginInputModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("Login", model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email)
                   ?? await _userManager.FindByNameAsync(model.Email);

        if (user is null || !user.IsEnabled)
        {
            ModelState.AddModelError(string.Empty, "Invalid credentials.");
            return View("Login", model);
        }

        var result = await _signInManager.PasswordSignInAsync(
            user.UserName ?? user.Email ?? model.Email,
            model.Password,
            model.RememberLogin,
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            // Re-issue auth cookie with explicit persistence when RememberLogin is set
            var props = new AuthenticationProperties
            {
                IsPersistent = model.RememberLogin,
                ExpiresUtc = model.RememberLogin
                    ? DateTimeOffset.UtcNow.AddDays(30)
                    : null
            };
            await _signInManager.SignInAsync(user, props);

            _logger.LogInformation("User {UserId} signed in via login UI. Persistent={Persistent}", user.Id, model.RememberLogin);
            return RedirectToLocal(model.ReturnUrl);
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "Account locked. Try again later.");
            _logger.LogWarning("User {UserId} is locked out.", user.Id);
        }
        else
        {
            ModelState.AddModelError(string.Empty, "Invalid credentials.");
            _logger.LogWarning("Invalid credentials for user {UserId}.", user.Id);
        }

        return View("Login", model);
    }

    [HttpGet("logout")]
    public async Task<IActionResult> Logout([FromQuery] string? logoutId)
    {
        await _signInManager.SignOutAsync();

        var logout = string.IsNullOrEmpty(logoutId)
            ? null
            : await _interaction.GetLogoutContextAsync(logoutId);

        if (!string.IsNullOrEmpty(logout?.PostLogoutRedirectUri))
        {
            return Redirect(logout.PostLogoutRedirectUri);
        }

        return Redirect("~/");
    }

    private IActionResult RedirectToLocal(string returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) &&
            (_interaction.IsValidReturnUrl(returnUrl) || Url.IsLocalUrl(returnUrl)))
        {
            return Redirect(returnUrl);
        }

        return Redirect("~/");
    }

    [HttpGet("external/google")]
    public IActionResult ExternalGoogle([FromQuery] string? returnUrl)
    {
        var redirectUrl = Url.Action(nameof(ExternalGoogleCallback), new { returnUrl });
        var props = _signInManager.ConfigureExternalAuthenticationProperties(
            GoogleDefaults.AuthenticationScheme,
            redirectUrl);

        return Challenge(props, GoogleDefaults.AuthenticationScheme);
    }

    [HttpGet("external/google/callback")]
    public async Task<IActionResult> ExternalGoogleCallback([FromQuery] string? returnUrl)
    {
        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            _logger.LogWarning("Google callback did not return external login info.");
            return RedirectToAction(nameof(Login), new { returnUrl });
        }

        var email = info.Principal?.FindFirstValue(ClaimTypes.Email) ?? "(no email)";
        var name = info.Principal?.Identity?.Name ?? "(no name)";

        // Clear the temporary external cookie
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        return Content($"Google authentication succeeded for {email} ({name}).");
    }
}

public sealed class LoginInputModel
{
    [Required]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public bool RememberLogin { get; set; } = true;

    public string ReturnUrl { get; set; } = string.Empty;
}
