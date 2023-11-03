using Microsoft.Extensions.Logging;

using KafkaFlow;

using Application.Common.Interfaces;

namespace Infrastructure.Kafka;

internal class RetryOrArchiveMiddleware : IMessageMiddleware
{
    private readonly ILogger<RetryOrArchiveMiddleware> _logger;
    private readonly IDeadLetterArchiver _deadLetterArchiver;

    public RetryOrArchiveMiddleware(
        ILogger<RetryOrArchiveMiddleware> logger,
        IDeadLetterArchiver deadLetterArchiver
    )
    {
        _logger = logger;
        _deadLetterArchiver = deadLetterArchiver;
    }

    public async Task Invoke(IMessageContext context, MiddlewareDelegate next)
    {
        int maxAttempts = 3; // @@TODO: Config.
        do
        {
            await next(context);

            byte[]? handleError = null;
            if ((handleError = context.Headers["HandleError"]) != null)
            {
                if (handleError[0] == 1) // retryable error
                {
                    var attemptsMade = (context.Headers["AttemptsMade"] ?? new byte[1] { 0 })[0] + 1;
                    if (attemptsMade < maxAttempts)
                    {
                        context.Headers["HandleError"] = null;
                        context.Headers["AttemptsMade"] = new byte[1] { (byte)attemptsMade };

                        _logger.LogWarning("A retryable error occured. Retrying...");

                        await Task.Delay(500); // @@TODO: Config.
                        continue;
                    }
                }

                _logger.LogWarning(
                    "An unretryable (or a retryable with max attempts exhausted) error occured. Putting into Dead-letter topic"
                );

                await _deadLetterArchiver.Archive(context.Message.Value, context.Headers);
            }

            return;
        } while (true);
    }
}
