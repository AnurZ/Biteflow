namespace Market.Application.Common.Exceptions;

public sealed class TenantContextMissingException : Exception
{
    public TenantContextMissingException(string message = "Tenant context is missing or invalid.") : base(message)
    {
    }
}
