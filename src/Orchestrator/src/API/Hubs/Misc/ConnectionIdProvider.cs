using Application.Common.Interfaces;

namespace API.Hubs.Misc;

public class ConnectionIdProvider : IConnectionIdProvider
{
    public string ConnectionId { get; set; }
}