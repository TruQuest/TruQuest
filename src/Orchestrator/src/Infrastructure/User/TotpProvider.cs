using System.Text;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using OtpNet;
using Nethereum.Hex.HexConvertors.Extensions;

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
        _logger.LogDebug("***** TotpSalt length: {TotpSaltLength}", _totpSaltBytes.Length);
    }

    Totp _getTotpGenerator(string address)
    {
        var addressBytes = address.HexToByteArray();
        var secret = new byte[_totpSaltBytes.Length + addressBytes.Length];
        _totpSaltBytes.CopyTo(secret, 0);
        addressBytes.CopyTo(secret, _totpSaltBytes.Length);

        return new Totp(secret);
    }

    public string GenerateTotpFor(string address) => _getTotpGenerator(address).ComputeTotp();

    public bool VerifyTotp(string address, string totp) =>
        _getTotpGenerator(address)
        .VerifyTotp(totp, out long _, window: VerificationWindow.RfcSpecifiedNetworkDelay);
}