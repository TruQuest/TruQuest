using System.IdentityModel.Tokens.Jwt;

using Microsoft.AspNetCore.SignalR;

namespace API.Hubs.Misc;

internal class UserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection) =>
        connection.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
}