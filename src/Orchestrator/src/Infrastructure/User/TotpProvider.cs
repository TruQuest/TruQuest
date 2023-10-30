using System.Text;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using OtpNet;

using Application.Common.Interfaces;

namespace Infrastructure.User;

internal class TotpProvider : ITotpProvider
{
    private readonly ILogger<TotpProvider> _logger;
    private readonly byte[] _totpSaltBytes;

    public TotpProvider(
        ILogger<TotpProvider> logger,
        IConfiguration configuration
    )
    {
        _logger = logger;
        var totpSalt = configuration["TotpSalt"]!;
        _totpSaltBytes = Encoding.UTF8.GetBytes(totpSalt);
    }

    Totp _getTotpGenerator(byte[] identifier)
    {
        var secret = new byte[_totpSaltBytes.Length + identifier.Length];
        _totpSaltBytes.CopyTo(secret, 0);
        identifier.CopyTo(secret, _totpSaltBytes.Length);

        return new Totp(secret, step: 120); // @@TODO: Config.
    }

    public string GenerateTotpFor(byte[] identifier) => _getTotpGenerator(identifier).ComputeTotp();

    public bool VerifyTotp(byte[] identifier, string totp) =>
        _getTotpGenerator(identifier)
        .VerifyTotp(totp, out long _, window: VerificationWindow.RfcSpecifiedNetworkDelay);
}
