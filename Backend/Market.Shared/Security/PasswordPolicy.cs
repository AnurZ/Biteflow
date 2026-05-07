namespace Market.Shared.Security;

public static class PasswordPolicy
{
    public const int RequiredLength = 10;
    public const int RequiredUniqueChars = 1;
    public const bool RequireDigit = true;
    public const bool RequireLowercase = true;
    public const bool RequireUppercase = true;
    public const bool RequireNonAlphanumeric = true;
}
