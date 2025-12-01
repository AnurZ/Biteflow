namespace Market.Application.Abstractions;

public interface ICaptchaVerifier
{
    Task<bool> VerifyAsync(string token, CancellationToken ct = default);
}
