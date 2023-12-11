using System.Text;

using KafkaFlow;

using Services;
using Messages.Responses;
using Common.Monitoring;

namespace Messages.Requests;

internal class ArchiveSettlementProposalAttachmentsCommand : BaseRequest
{
    public required string SubmitterId { get; init; }
    public required Guid ProposalId { get; init; }
    public required NewSettlementProposalIm Input { get; init; }

    public override IEnumerable<(string Name, object? Value)> GetActivityTags()
    {
        return new (string Name, object? Value)[]
        {
            (ActivityTags.UserId, SubmitterId),
            (ActivityTags.ThingId, Input.ThingId),
            (ActivityTags.SettlementProposalId, ProposalId)
        };
    }
}

internal class ArchiveSettlementProposalAttachmentsCommandHandler :
    IMessageHandler<ArchiveSettlementProposalAttachmentsCommand>
{
    private readonly ILogger<ArchiveSettlementProposalAttachmentsCommandHandler> _logger;
    private readonly IFileArchiver _fileArchiver;
    private readonly IResponseDispatcher _responseDispatcher;

    public ArchiveSettlementProposalAttachmentsCommandHandler(
        ILogger<ArchiveSettlementProposalAttachmentsCommandHandler> logger,
        IFileArchiver fileArchiver,
        IResponseDispatcher responseDispatcher
    )
    {
        _logger = logger;
        _fileArchiver = fileArchiver;
        _responseDispatcher = responseDispatcher;
    }

    public async Task Handle(IMessageContext context, ArchiveSettlementProposalAttachmentsCommand message)
    {
        var requestId = Encoding.UTF8.GetString(context.Headers["trq.requestId"]);

        var progress = new Progress<int>(percent =>
        {
            _logger.LogDebug($"Archive Progress: {percent}%");
            _responseDispatcher.SendSync(
                requestId,
                new ArchiveSettlementProposalAttachmentsProgress
                {
                    SubmitterId = message.SubmitterId,
                    ProposalId = message.ProposalId,
                    Percent = percent
                },
                key: message.ProposalId.ToString()
            );
        });

        object response;
        var error = await _fileArchiver.ArchiveAllAttachments(requestId, message.Input, progress);
        if (error != null)
        {
            response = new ArchiveSettlementProposalAttachmentsFailureResult
            {
                SubmitterId = message.SubmitterId,
                ProposalId = message.ProposalId,
                ErrorMessage = error.ToString()
            };
        }
        else
        {
            response = new ArchiveSettlementProposalAttachmentsSuccessResult
            {
                SubmitterId = message.SubmitterId,
                ProposalId = message.ProposalId,
                Input = message.Input
            };
        }

        await _responseDispatcher.Send(requestId, response, key: message.ProposalId.ToString());
    }
}
