using GoThataway;

using Domain.Results;

using Application.Common.Interfaces;
using Application.Common.Models.IM;

namespace Application.Common.Middlewares.Request;

public class MultipartFormReceivingMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : HandleResult, new()
{
    private readonly IFileReceiver _fileReceiver;
    private readonly ICurrentPrincipal _currentPrincipal;

    public MultipartFormReceivingMiddleware(
        IFileReceiver fileReceiver,
        ICurrentPrincipal currentPrincipal
    )
    {
        _fileReceiver = fileReceiver;
        _currentPrincipal = currentPrincipal;
    }

    public async Task<TResponse> Handle(TRequest request, Func<Task<TResponse>> next, CancellationToken ct)
    {
        if (request is ManuallyBoundInputModelCommand command)
        {
            var result = await _fileReceiver.ReceiveFilesAndFormValues(
                command.Request,
                maxSize: 10 * 1024 * 1024, // @@TODO: Config.
                filePrefix: command.RequestId
            );
            if (result.IsError)
            {
                return new()
                {
                    Error = result.Error
                };
            }

            command.Input.BindFrom(result.Data!);
        }

        return await next();
    }
}
