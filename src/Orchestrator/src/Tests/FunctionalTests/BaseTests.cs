using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;

using MediatR;

using Tests.FunctionalTests.Helpers;

namespace Tests.FunctionalTests;

public class BaseTests : IAsyncLifetime
{
    protected Sut _sut;
    protected EventBroadcaster _eventBroadcaster;

    private ClaimsPrincipal? _user;

    protected readonly string _dummyQuillContentJson;
    private readonly List<Dictionary<string, object>> _dummyQuillContent = new()
    {
        new()
        {
            ["insert"] = "Hello World!"
        },
        new()
        {
            ["attributes"] = new Dictionary<string, object>()
            {
                ["header"] = 1
            },
            ["insert"] = "\n"
        },
        new()
        {
            ["insert"] = "Welcome to TruQuest!"
        },
        new()
        {
            ["attributes"] = new Dictionary<string, object>()
            {
                ["header"] = 2
            },
            ["insert"] = "\n"
        }
    };

    public BaseTests()
    {
        _dummyQuillContentJson = JsonSerializer.Serialize(_dummyQuillContent);
    }

    public virtual async Task InitializeAsync()
    {
        _sut = await Sut.GetOrInit();

        _eventBroadcaster = new();
        _sut.ApplicationEventChannel.RegisterConsumer(_eventBroadcaster.EventSink);
        _sut.ApplicationRequestChannel.RegisterConsumer(_eventBroadcaster.RequestSink);
        _eventBroadcaster.Start();
    }

    public virtual async Task DisposeAsync()
    {
        _sut.ApplicationEventChannel.UnregisterConsumer(_eventBroadcaster.EventSink);
        _sut.ApplicationRequestChannel.UnregisterConsumer(_eventBroadcaster.RequestSink);
        _eventBroadcaster.Stop();
    }

    protected async Task _runAs(string accountName)
    {
        _user = new ClaimsPrincipal(new ClaimsIdentity(
            new Claim[]
            {
                new(JwtRegisteredClaimNames.Sub, _sut.AccountNameToUserId[accountName]),
                new("signer_address", _sut.AccountProvider.GetAccount(accountName).Address),
                new("wallet_address", await _sut.ContractCaller.GetWalletAddressFor(accountName))
            },
            "Bearer"
        ));
    }

    protected void _runAsGuest()
    {
        _user = null;
    }

    protected Task<TResponse> _sendRequest<TResponse>(IRequest<TResponse> request) => _sut.SendRequestAs(request, _user);
}
