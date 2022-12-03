using System.Threading.Channels;
using System.Runtime.CompilerServices;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

using MediatR;
using Nethereum.Web3;
using Nethereum.Contracts;
using Nethereum.BlockchainProcessing.Processor;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.ABI.FunctionEncoding.Attributes;

using Application.Common.Interfaces;
using AppEvents = Application.Ethereum.Events;

using Infrastructure.Ethereum.Events;

namespace Infrastructure.Ethereum;

internal class ContractEventListener : IContractEventListener
{
    private readonly ILogger<ContractEventListener> _logger;
    private readonly Web3 _web3;
    private readonly uint _blockConfirmations;
    private readonly string _truQuestAddress;
    private readonly string _verifierLotteryAddress;

    private readonly ChannelReader<IEventLog> _stream;
    private readonly ChannelWriter<IEventLog> _sink;

    public ContractEventListener(IConfiguration configuration, ILogger<ContractEventListener> logger)
    {
        _logger = logger;
        var network = configuration["Ethereum:Network"]!;
        _web3 = new Web3(configuration[$"Ethereum:Networks:{network}:URL"]);
        _blockConfirmations = configuration.GetValue<uint>($"Ethereum:Networks:{network}:BlockConfirmations");
        _truQuestAddress = configuration[$"Ethereum:Contracts:{network}:TruQuest:Address"]!;
        _verifierLotteryAddress = configuration[$"Ethereum:Contracts:{network}:VerifierLottery:Address"]!;

        var channel = Channel.CreateUnbounded<IEventLog>(new UnboundedChannelOptions
        {
            SingleReader = true
        });
        _stream = channel.Reader;
        _sink = channel.Writer;
    }

    public async IAsyncEnumerable<INotification> GetNext([EnumeratorCancellation] CancellationToken stoppingToken)
    {
        new Thread(async () =>
        {
            async Task WriteToChannel<T>(EventLog<T> @event) where T : IEventDTO
            {
                await _sink.WriteAsync(@event);
            }

            var thingFundedEventHandler = new EventLogProcessorHandler<ThingFundedEvent>(WriteToChannel);
            var preJoinedLotteryEventHandler = new EventLogProcessorHandler<PreJoinedLotteryEvent>(WriteToChannel);
            var joinedLotteryEventHandler = new EventLogProcessorHandler<JoinedLotteryEvent>(WriteToChannel);
            var lotteryClosedWithSuccessEventHandler = new EventLogProcessorHandler<LotteryClosedWithSuccessEvent>(WriteToChannel);

            var eventHandlers = new ProcessorHandler<FilterLog>[]
            {
                thingFundedEventHandler,
                preJoinedLotteryEventHandler,
                joinedLotteryEventHandler,
                lotteryClosedWithSuccessEventHandler
            };

            var contractFilter = new NewFilterInput
            {
                Address = new[] { _truQuestAddress, _verifierLotteryAddress }
            };

            var logProcessor = _web3.Processing.Logs.CreateProcessor(
                logProcessors: eventHandlers,
                filter: contractFilter,
                minimumBlockConfirmations: _blockConfirmations,
                log: _logger
            );
            // @@TODO: Add block progress repo.

            try
            {
                await logProcessor.ExecuteAsync(cancellationToken: stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _sink.Complete();
            }
        }).Start();

        await foreach (var @event in _stream.ReadAllAsync())
        {
            if (@event is EventLog<ThingFundedEvent> thingFundedEvent)
            {
                yield return new AppEvents.ThingFunded.ThingFundedEvent
                {
                    BlockNumber = (long)thingFundedEvent.Log.BlockNumber.Value,
                    TxnIndex = (int)thingFundedEvent.Log.TransactionIndex.Value,
                    ThingIdHash = thingFundedEvent.Event.ThingIdHash.Replace("0x", string.Empty),
                    UserId = thingFundedEvent.Event.UserId.Replace("0x", string.Empty),
                    Stake = (decimal)thingFundedEvent.Event.Stake
                };
            }
            else if (@event is EventLog<PreJoinedLotteryEvent> preJoinedVerifierLotteryEvent)
            {
                yield return new AppEvents.Lottery.PreJoinedVerifierLottery.PreJoinedVerifierLotteryEvent
                {
                    BlockNumber = (long)preJoinedVerifierLotteryEvent.Log.BlockNumber.Value,
                    TxnIndex = (int)preJoinedVerifierLotteryEvent.Log.TransactionIndex.Value,
                    ThingIdHash = preJoinedVerifierLotteryEvent.Event.ThingIdHash.Replace("0x", string.Empty),
                    UserId = preJoinedVerifierLotteryEvent.Event.UserId.Replace("0x", string.Empty),
                    DataHash = preJoinedVerifierLotteryEvent.Event.DataHash
                };
            }
            else if (@event is EventLog<JoinedLotteryEvent> joinedVerifierLotteryEvent)
            {
                yield return new AppEvents.Lottery.JoinedVerifierLottery.JoinedVerifierLotteryEvent
                {
                    BlockNumber = (long)joinedVerifierLotteryEvent.Log.BlockNumber.Value,
                    TxnIndex = (int)joinedVerifierLotteryEvent.Log.TransactionIndex.Value,
                    ThingIdHash = joinedVerifierLotteryEvent.Event.ThingIdHash.Replace("0x", string.Empty),
                    UserId = joinedVerifierLotteryEvent.Event.UserId.Replace("0x", string.Empty),
                    Nonce = joinedVerifierLotteryEvent.Event.Nonce
                };
            }
            else if (@event is EventLog<LotteryClosedWithSuccessEvent> verifierLotteryClosedWithSuccessEvent)
            {
                yield return new AppEvents.Lottery.VerifierLotteryClosedWithSuccess.VerifierLotteryClosedWithSuccessEvent
                {
                    ThingIdHash = verifierLotteryClosedWithSuccessEvent.Event.ThingIdHash.Replace("0x", string.Empty),
                    WinnerIds = verifierLotteryClosedWithSuccessEvent.Event.WinnerIds
                };
            }
        }
    }
}