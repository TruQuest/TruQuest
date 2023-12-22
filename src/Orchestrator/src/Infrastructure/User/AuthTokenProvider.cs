using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

using Application.Common.Interfaces;

namespace Infrastructure.User;

public class AuthTokenProvider : IAuthTokenProvider
{
    private readonly SigningCredentials _credentials;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expiresInDays;

    public AuthTokenProvider(IConfiguration configuration)
    {
        var jwtConfig = configuration.GetSection("JWT");

        var rsaPrivateKey = RSA.Create(); // @@??: Important to not dispose?
        rsaPrivateKey.FromXmlString(jwtConfig["PrivateKey"]!);
        _credentials = new SigningCredentials(
            key: new RsaSecurityKey(rsaPrivateKey),
            algorithm: SecurityAlgorithms.RsaSha256
        );

        _issuer = jwtConfig["Issuer"]!;
        _audience = jwtConfig["Audience"]!;
        _expiresInDays = jwtConfig.GetValue<int>("ExpiresInDays");
    }

    public string GenerateJwt(string id, IList<Claim>? claims = null)
    {
        claims ??= new List<Claim>();
        claims.Add(new Claim(JwtRegisteredClaimNames.Sub, id));

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(_expiresInDays),
            signingCredentials: _credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
