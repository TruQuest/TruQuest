using Microsoft.AspNetCore.SignalR;

public class DummyHub : Hub
{
    private readonly ILogger<DummyHub> _logger;

    public DummyHub(ILogger<DummyHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
        _logger.LogInformation("=============== Connected! ===============");
    }

    public async Task<string> Do(string arg)
    {
        _logger.LogInformation($"=============== Do {arg} ===============");
        return "Done!";
    }
}
