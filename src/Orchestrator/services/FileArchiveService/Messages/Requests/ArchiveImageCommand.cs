using System.Text;

using KafkaFlow;
using KafkaFlow.TypedHandler;

using Messages.Responses;
using Services;

namespace Messages.Requests;

internal class ArchiveImageCommand
{
    public string Url { get; set; }
}

internal class ArchiveImageCommandHandler : IMessageHandler<ArchiveImageCommand>
{
    private readonly ILogger<ArchiveImageCommandHandler> _logger;
    private readonly IImageSaver _imageSaver;
    private readonly IFileStorage _fileStorage;
    private readonly IResponseDispatcher _responseDispatcher;

    public ArchiveImageCommandHandler(
        ILogger<ArchiveImageCommandHandler> logger,
        IImageSaver imageSaver,
        IFileStorage fileStorage,
        IResponseDispatcher responseDispatcher
    )
    {
        _logger = logger;
        _imageSaver = imageSaver;
        _fileStorage = fileStorage;
        _responseDispatcher = responseDispatcher;
    }

    public async Task Handle(IMessageContext context, ArchiveImageCommand message)
    {
        object response;
        var saveResult = await _imageSaver.SaveLocalCopy(message.Url);
        if (saveResult.IsError)
        {
            _logger.LogWarning(saveResult.Error!.ToString());
            response = new ArchiveImageFailureResult
            {
                ErrorMessage = saveResult.Error.ToString()
            };
        }
        else
        {
            var storeResult = await _fileStorage.Upload(saveResult.Data!);
            // File.Delete(saveResult.Data!);
            if (storeResult.IsError)
            {
                _logger.LogWarning(storeResult.Error!.ToString());
                response = new ArchiveImageFailureResult
                {
                    ErrorMessage = storeResult.Error.ToString()
                };
            }
            else
            {
                response = new ArchiveImageSuccessResult
                {
                    IpfsCid = storeResult.Data!
                };
            }
        }

        await _responseDispatcher.DispatchFor(
            Encoding.UTF8.GetString(context.Headers["requestId"]),
            response
        );
    }
}