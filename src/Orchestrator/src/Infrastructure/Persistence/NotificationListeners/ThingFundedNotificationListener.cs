using System.Data;
using System.Threading.Channels;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Npgsql;

using Application.Common.Interfaces;

namespace Infrastructure.Persistence.NotificationListeners;

internal class ThingFundedNotificationListener : IThingFundedNotificationListener
{
    private readonly ILogger<ThingFundedNotificationListener> _logger;
    private readonly string _dbConnectionString;
    private NpgsqlConnection? _dbConnection;
    private readonly ChannelReader<string> _stream;
    private readonly ChannelWriter<string> _sink;

    public ThingFundedNotificationListener(
        ILogger<ThingFundedNotificationListener> logger,
        IConfiguration configuration
    )
    {
        _logger = logger;
        _dbConnectionString = configuration.GetConnectionString("Postgres") + "SearchPath=truquest_events;";

        var channel = Channel.CreateUnbounded<string>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = true
        });
        _stream = channel.Reader;
        _sink = channel.Writer;
    }

    private async ValueTask<NpgsqlConnection> _ensureConnectionOpen()
    {
        if (_dbConnection == null)
        {
            _dbConnection = new NpgsqlConnection(_dbConnectionString);
        }
        if (_dbConnection.State != ConnectionState.Open)
        {
            await _dbConnection.OpenAsync();
        }

        return _dbConnection;
    }

    public void Dispose() => _dbConnection?.Dispose();

    public IAsyncEnumerable<string> GetNext(CancellationToken stoppingToken)
    {
        new Thread(async () =>
        {
            try
            {
                var cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);

                await _ensureConnectionOpen();

                _dbConnection!.Notification += (_, eventArgs) => _sink.TryWrite(eventArgs.Payload);

                _dbConnection.StateChange += (_, eventArgs) =>
                {
                    if (eventArgs.CurrentState is ConnectionState.Closed or ConnectionState.Broken)
                    {
                        if (!cts.IsCancellationRequested)
                        {
                            cts.Cancel();
                        }
                    }
                };

                await using (var cmd = new NpgsqlCommand("LISTEN event_channel", _dbConnection))
                {
                    await cmd.ExecuteNonQueryAsync();
                }

                while (true)
                {
                    await _dbConnection.WaitAsync(cts.Token);
                }
            }
            catch (OperationCanceledException)
            {
                _sink.Complete();
            }
        }).Start();

        return _stream.ReadAllAsync();
    }
}