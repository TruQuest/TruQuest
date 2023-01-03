using System.Text;

using KafkaFlow;
using KafkaFlow.TypedHandler;

using Messages.Responses;
using Services;

namespace Messages.Requests;

internal class ArchiveWebPageCommand
{
    public string Url { get; set; }
}

internal class ArchiveWebPageCommandHandler : IMessageHandler<ArchiveWebPageCommand>
{
    private readonly ILogger<ArchiveWebPageCommandHandler> _logger;
    private readonly IWebPageSaver _webPageSaver;
    private readonly IFileStorage _fileStorage;
    private readonly IResponseDispatcher _responseDispatcher;

    public ArchiveWebPageCommandHandler(
        ILogger<ArchiveWebPageCommandHandler> logger,
        IWebPageSaver webPageSaver,
        IFileStorage fileStorage,
        IResponseDispatcher responseDispatcher
    )
    {
        _logger = logger;
        _webPageSaver = webPageSaver;
        _fileStorage = fileStorage;
        _responseDispatcher = responseDispatcher;
    }

    public async Task Handle(IMessageContext context, ArchiveWebPageCommand message)
    {
        var filePath = await _webPageSaver.SaveLocalCopy(message.Url);
        var result = await _fileStorage.Upload(filePath);
        // File.Delete(filePath);

        object response;
        if (result.IsError)
        {
            _logger.LogWarning(result.Error!.ToString());
            response = new ArchiveWebPageFailureResult { ErrorMessage = result.Error.ToString() };
        }
        else
        {
            response = new ArchiveWebPageSuccessResult { IpfsCid = result.Data! };
        }

        await _responseDispatcher.DispatchFor(
            Encoding.UTF8.GetString(context.Headers["requestId"]),
            response
        );
    }
}