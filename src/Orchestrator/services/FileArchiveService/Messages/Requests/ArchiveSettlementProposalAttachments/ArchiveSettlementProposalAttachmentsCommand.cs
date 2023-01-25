using KafkaFlow;
using KafkaFlow.TypedHandler;

using Messages.Responses;
using Services;

namespace Messages.Requests;

internal class ArchiveSettlementProposalAttachmentsCommand
{
    public required string SubmitterId { get; init; }
    public required Guid ProposalId { get; init; }
    public required NewSettlementProposalIm Input { get; init; }
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
        var progress = new Progress<int>(percent =>
        {
            _logger.LogInformation($"Archive Progress: {percent}%");
            _responseDispatcher.Send(
                new ArchiveSettlementProposalAttachmentsProgress
                {
                    SubmitterId = message.SubmitterId,
                    ProposalId = message.ProposalId,
                    Percent = percent
                },
                key: message.ProposalId.ToString()
            );
        });

        var error = await _fileArchiver.ArchiveAllAttachments(message.Input, progress);
        object response;
        if (error != null)
        {
            response = new ArchiveSettlementProposalAttachmentsFailureResult
            {
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

        await _responseDispatcher.SendAsync(response, key: message.ProposalId.ToString());
    }
}