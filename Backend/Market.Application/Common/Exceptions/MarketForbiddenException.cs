namespace Market.Application.Common.Exceptions;

public sealed class MarketForbiddenException : Exception
{
    public MarketForbiddenException(string message) : base(message)
    {
    }
}
