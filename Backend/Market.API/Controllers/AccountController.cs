using System.ComponentModel.DataAnnotations;
using Duende.IdentityServer.Services;
using Market.Domain.Entities.IdentityV2;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Duende.IdentityModel;
using Market.Application.Abstractions;
using Market.Shared.Constants;
using Microsoft.AspNetCore.Authentication.Google;

[AllowAnonymous]
[Route("account")]
public sealed class AccountController : Controller
{
    private const string InvalidLoginMessage = "Neispravni podaci za prijavu.";
    private const string TenantIdStateKey = "tenant_id";
    private const string RestaurantIdStateKey = "restaurant_id";
    private static readonly string DummyPasswordHash =
        new PasswordHasher<ApplicationUser>().HashPassword(new ApplicationUser(), "DummyPasswordForTimingOnly");

    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IIdentityServerInteractionService _interaction;
    private readonly IPublicTenantResolver _publicTenantResolver;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        IIdentityServerInteractionService interaction,
        IPublicTenantResolver publicTenantResolver,
        ILogger<AccountController> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _interaction = interaction;
        _publicTenantResolver = publicTenantResolver;
        _logger = logger;
    }

    [HttpGet("login")]
    public async Task<IActionResult> Login([FromQuery] string? returnUrl)
    {
        var ctx = string.IsNullOrWhiteSpace(returnUrl)
            ? null
            : await _interaction.GetAuthorizationContextAsync(returnUrl);

        var idp = ctx?.IdP;
        if (string.IsNullOrWhiteSpace(idp) && !string.IsNullOrWhiteSpace(returnUrl))
        {
            var idx = returnUrl.IndexOf('?', StringComparison.Ordinal);
            if (idx >= 0)
            {
                var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(returnUrl[idx..]);
                if (query.TryGetValue("idp", out var idpValues))
                {
                    idp = idpValues.FirstOrDefault();
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(idp))
        {
            // Single external provider requested; short-circuit to Google flow
            if (string.Equals(idp, "Google", StringComparison.OrdinalIgnoreCase))
            {
                var tenantState = ResolveTenantState(returnUrl, Request.Query);
                return RedirectToAction(nameof(ExternalGoogle), new
                {
                    returnUrl,
                    tenantId = tenantState?.TenantId,
                    restaurantId = tenantState?.RestaurantId
                });
            }
        }

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

        if (user is null)
        {
            VerifyDummyPassword(model.Password);
            _logger.LogWarning("Login UI rejected because user '{Email}' was not found.", model.Email);
            return InvalidLoginView(model);
        }

        if (!user.IsEnabled)
        {
            VerifyDummyPassword(model.Password);
            _logger.LogWarning("Login UI rejected because user {UserId} is disabled.", user.Id);
            return InvalidLoginView(model);
        }

        var result = await _signInManager.CheckPasswordSignInAsync(
            user,
            model.Password,
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
            _logger.LogWarning("User {UserId} is locked out.", user.Id);
        }
        else
        {
            _logger.LogWarning("Invalid credentials for user {UserId}.", user.Id);
        }

        return InvalidLoginView(model);
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

    private IActionResult InvalidLoginView(LoginInputModel model)
    {
        ModelState.AddModelError(string.Empty, InvalidLoginMessage);
        return View("Login", model);
    }

    private static void VerifyDummyPassword(string? password)
    {
        var hasher = new PasswordHasher<ApplicationUser>();
        hasher.VerifyHashedPassword(new ApplicationUser(), DummyPasswordHash, password ?? string.Empty);
    }

    [HttpGet("external/google")]
    public IActionResult ExternalGoogle(
        [FromQuery] string? returnUrl,
        [FromQuery] Guid? tenantId,
        [FromQuery] Guid? restaurantId)
    {
        var callback = Url.Action(nameof(ExternalGoogleCallback), new { returnUrl });
        var props = _signInManager.ConfigureExternalAuthenticationProperties(
            GoogleDefaults.AuthenticationScheme,
            callback);

        if (TryNormalizeTenantState(tenantId, restaurantId, out var normalizedTenantId, out var normalizedRestaurantId))
        {
            props.Items[TenantIdStateKey] = normalizedTenantId.ToString();
            props.Items[RestaurantIdStateKey] = normalizedRestaurantId.ToString();
        }

        return Challenge(props, GoogleDefaults.AuthenticationScheme);
    }

    [HttpGet("external/google/callback")]
    public async Task<IActionResult> ExternalGoogleCallback([FromQuery] string? returnUrl)
    {
        // Retrieve info from the external provider
        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            _logger.LogWarning("External login info missing in Google callback.");
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            return RedirectToAction(nameof(Login), new { returnUrl });
        }

        var email = info.Principal?.FindFirstValue(ClaimTypes.Email)
                    ?? info.Principal?.FindFirstValue(JwtClaimTypes.Email);

        if (string.IsNullOrWhiteSpace(email))
        {
            _logger.LogWarning("External login rejected because email is missing. Provider={Provider}, Key={Key}", info.LoginProvider, info.ProviderKey);
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            return RedirectToAction(nameof(Login), new { returnUrl });
        }

        // Try to find user by external login or email
        var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey)
                   ?? await _userManager.FindByEmailAsync(email);

        if (user == null)
        {
            if (!TryGetTenantState(info.AuthenticationProperties, out var tenantId, out var restaurantId))
            {
                _logger.LogWarning("Google login rejected because tenant state is missing or invalid for {Email}.", email);
                await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
                return RedirectToAction(nameof(Login), new { returnUrl });
            }

            PublicTenantContext publicTenant;
            try
            {
                publicTenant = await _publicTenantResolver.ResolveRequiredAsync(tenantId, restaurantId, HttpContext.RequestAborted);
            }
            catch (FluentValidation.ValidationException ex)
            {
                _logger.LogWarning(ex, "Google login rejected because tenant state could not be validated for {Email}.", email);
                await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
                return RedirectToAction(nameof(Login), new { returnUrl });
            }

            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                DisplayName = info.Principal?.Identity?.Name ?? email,
                TenantId = publicTenant.TenantId,
                RestaurantId = publicTenant.RestaurantId,
                IsEnabled = true
            };

            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                _logger.LogWarning("Failed creating user from Google login: {Errors}", string.Join(", ", createResult.Errors.Select(e => e.Description)));
                await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
                return RedirectToAction(nameof(Login), new { returnUrl });
            }
        }
        else
        {
            // Ensure we have a display name saved
            if (string.IsNullOrWhiteSpace(user.DisplayName))
            {
                user.DisplayName = info.Principal?.Identity?.Name ?? email;
                await _userManager.UpdateAsync(user);
            }
        }

        // Link external login if not already linked
        var existingLogins = await _userManager.GetLoginsAsync(user);
        if (!existingLogins.Any(l => l.LoginProvider == info.LoginProvider && l.ProviderKey == info.ProviderKey))
        {
            var addLogin = await _userManager.AddLoginAsync(user, info);
            if (!addLogin.Succeeded)
            {
                _logger.LogWarning("Failed adding external login for {UserId}: {Errors}", user.Id, string.Join(", ", addLogin.Errors.Select(e => e.Description)));
            }
        }

        // Ensure customer role
        if (!await _userManager.IsInRoleAsync(user, RoleNames.Customer))
        {
            var addRole = await _userManager.AddToRoleAsync(user, RoleNames.Customer);
            if (!addRole.Succeeded)
            {
                _logger.LogWarning("Failed adding customer role for {UserId}: {Errors}", user.Id, string.Join(", ", addRole.Errors.Select(e => e.Description)));
            }
        }

        // Sign the user in locally
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
        await _signInManager.SignInAsync(user, isPersistent: false, authenticationMethod: info.LoginProvider);

        return RedirectToLocal(returnUrl ?? string.Empty);
    }

    private static GoogleTenantState? ResolveTenantState(string? returnUrl, IQueryCollection query)
    {
        if (TryReadTenantState(query, out var directTenantId, out var directRestaurantId))
        {
            return new GoogleTenantState(directTenantId, directRestaurantId);
        }

        if (string.IsNullOrWhiteSpace(returnUrl))
        {
            return null;
        }

        var idx = returnUrl.IndexOf('?', StringComparison.Ordinal);
        if (idx < 0)
        {
            return null;
        }

        var returnQuery = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(returnUrl[idx..]);
        if (TryReadTenantState(returnQuery, out var tenantId, out var restaurantId))
        {
            return new GoogleTenantState(tenantId, restaurantId);
        }

        return TryReadTenantStateFromOAuthState(returnQuery, out tenantId, out restaurantId)
            ? new GoogleTenantState(tenantId, restaurantId)
            : null;
    }

    private static bool TryReadTenantState(
        IEnumerable<KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues>> values,
        out Guid tenantId,
        out Guid restaurantId)
    {
        tenantId = Guid.Empty;
        restaurantId = Guid.Empty;

        var tenantRaw = ReadFirst(values, "tenantId") ?? ReadFirst(values, TenantIdStateKey);
        var restaurantRaw = ReadFirst(values, "restaurantId") ?? ReadFirst(values, RestaurantIdStateKey);

        return Guid.TryParse(tenantRaw, out tenantId) &&
               Guid.TryParse(restaurantRaw, out restaurantId) &&
               tenantId != Guid.Empty &&
               restaurantId != Guid.Empty;
    }

    private static bool TryReadTenantStateFromOAuthState(
        IEnumerable<KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues>> values,
        out Guid tenantId,
        out Guid restaurantId)
    {
        tenantId = Guid.Empty;
        restaurantId = Guid.Empty;

        var state = ReadFirst(values, "state");
        if (string.IsNullOrWhiteSpace(state))
        {
            return false;
        }

        var separatorIndex = state.IndexOf(';');
        if (separatorIndex < 0 || separatorIndex == state.Length - 1)
        {
            return false;
        }

        var additionalState = Uri.UnescapeDataString(state[(separatorIndex + 1)..]);
        var queryIndex = additionalState.IndexOf('?', StringComparison.Ordinal);
        if (queryIndex < 0)
        {
            return false;
        }

        var stateQuery = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(additionalState[queryIndex..]);
        return TryReadTenantState(stateQuery, out tenantId, out restaurantId);
    }

    private static string? ReadFirst(
        IEnumerable<KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues>> values,
        string key)
    {
        foreach (var pair in values)
        {
            if (string.Equals(pair.Key, key, StringComparison.OrdinalIgnoreCase))
            {
                return pair.Value.FirstOrDefault();
            }
        }

        return null;
    }

    private static bool TryNormalizeTenantState(
        Guid? tenantId,
        Guid? restaurantId,
        out Guid normalizedTenantId,
        out Guid normalizedRestaurantId)
    {
        normalizedTenantId = tenantId ?? Guid.Empty;
        normalizedRestaurantId = restaurantId ?? Guid.Empty;

        return normalizedTenantId != Guid.Empty && normalizedRestaurantId != Guid.Empty;
    }

    private static bool TryGetTenantState(AuthenticationProperties? properties, out Guid tenantId, out Guid restaurantId)
    {
        tenantId = Guid.Empty;
        restaurantId = Guid.Empty;

        if (properties is null)
        {
            return false;
        }

        return properties.Items.TryGetValue(TenantIdStateKey, out var tenantRaw) &&
               properties.Items.TryGetValue(RestaurantIdStateKey, out var restaurantRaw) &&
               Guid.TryParse(tenantRaw, out tenantId) &&
               Guid.TryParse(restaurantRaw, out restaurantId) &&
               tenantId != Guid.Empty &&
               restaurantId != Guid.Empty;
    }

    private sealed record GoogleTenantState(Guid TenantId, Guid RestaurantId);
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
