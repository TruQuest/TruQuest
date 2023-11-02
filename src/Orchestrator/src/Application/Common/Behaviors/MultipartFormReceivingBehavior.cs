using MediatR;

using Domain.Results;

using Application.Common.Interfaces;
using Application.Common.Models.IM;

namespace Application.Common.Behaviors;

public class MultipartFormReceivingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : HandleResult, new()
{
    private readonly IFileReceiver _fileReceiver;
    private readonly ICurrentPrincipal _currentPrincipal;

    public MultipartFormReceivingBehavior(
        IFileReceiver fileReceiver,
        ICurrentPrincipal currentPrincipal
    )
    {
        _fileReceiver = fileReceiver;
        _currentPrincipal = currentPrincipal;
    }

    public async Task<TResponse> Handle(
        TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct
    )
    {
        if (request is ManuallyBoundInputModelCommand command)
        {
            var result = await _fileReceiver.ReceiveFilesAndFormValues(
                command.Request,
                maxSize: 10 * 1024 * 1024, // @@TODO: Config.
                filePrefix: _currentPrincipal.Id!
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
