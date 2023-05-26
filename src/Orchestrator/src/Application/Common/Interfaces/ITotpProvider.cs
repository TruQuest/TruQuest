namespace Application.Common.Interfaces;

public interface ITotpProvider
{
    string GenerateTotpFor(string address);
    bool VerifyTotp(string address, string totp);
}