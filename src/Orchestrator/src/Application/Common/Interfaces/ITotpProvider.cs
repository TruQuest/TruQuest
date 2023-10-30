namespace Application.Common.Interfaces;

public interface ITotpProvider
{
    string GenerateTotpFor(byte[] identifier);
    bool VerifyTotp(byte[] identifier, string totp);
}
