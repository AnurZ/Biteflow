using Market.Application.Modules.Auth.Commands.RegisterCustomer;
using Market.Application.Modules.Staff.Commands.Create;
using Market.Shared.Constants;
using Market.Shared.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Market.Tests.AuthTests;

public sealed class PasswordPolicyValidatorTests
{
    private static readonly IOptions<IdentityOptions> IdentityOptions = Options.Create(new IdentityOptions
    {
        Password =
        {
            RequiredLength = PasswordPolicy.RequiredLength,
            RequiredUniqueChars = PasswordPolicy.RequiredUniqueChars,
            RequireDigit = PasswordPolicy.RequireDigit,
            RequireLowercase = PasswordPolicy.RequireLowercase,
            RequireUppercase = PasswordPolicy.RequireUppercase,
            RequireNonAlphanumeric = PasswordPolicy.RequireNonAlphanumeric
        }
    });

    [Fact]
    public void RegisterCustomer_ShouldRejectWeakPassword()
    {
        var validator = new RegisterCustomerCommandValidator(IdentityOptions);

        var result = validator.Validate(new RegisterCustomerCommand
        {
            Email = "customer@example.test",
            Password = "lowercase",
            DisplayName = "Customer",
            CaptchaToken = "captcha-token"
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(RegisterCustomerCommand.Password));
    }

    [Fact]
    public void RegisterCustomer_ShouldAcceptCompliantPassword()
    {
        var validator = new RegisterCustomerCommandValidator(IdentityOptions);

        var result = validator.Validate(new RegisterCustomerCommand
        {
            Email = "customer@example.test",
            Password = "Customer123!",
            DisplayName = "Customer",
            CaptchaToken = "captcha-token"
        });

        Assert.True(result.IsValid, string.Join(", ", result.Errors.Select(x => x.ErrorMessage)));
    }

    [Fact]
    public void CreateStaff_ShouldRejectWeakPlainPassword()
    {
        var validator = new CreateStaffCommandValidator(IdentityOptions);

        var result = validator.Validate(CreateStaffRequest("lowercase"));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(CreateStaffCommand.PlainPassword));
    }

    [Fact]
    public void CreateStaff_ShouldAcceptCompliantPlainPassword()
    {
        var validator = new CreateStaffCommandValidator(IdentityOptions);

        var result = validator.Validate(CreateStaffRequest("StaffPass123!"));

        Assert.True(result.IsValid, string.Join(", ", result.Errors.Select(x => x.ErrorMessage)));
    }

    private static CreateStaffCommand CreateStaffRequest(string password)
        => new()
        {
            Email = "staff@example.test",
            DisplayName = "Staff User",
            PlainPassword = password,
            Role = RoleNames.Admin,
            FirstName = "Staff",
            LastName = "User",
            PhoneNumber = "123456",
            HireDate = DateTime.UtcNow.Date,
            HourlyRate = 10m,
            EmploymentType = "FullTime",
            ShiftType = "Morning",
            ShiftStart = new TimeOnly(8, 0),
            ShiftEnd = new TimeOnly(16, 0),
            IsActive = true,
            Notes = "Validator test"
        };
}
