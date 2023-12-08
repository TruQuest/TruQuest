using Microsoft.AspNetCore.Http;

using Application.Common.Interfaces;

namespace Application.Common.Models.IM;

public abstract class ManuallyBoundInputModelCommand
{
    public string RequestId { get; }
    public HttpRequest Request { get; }
    public IManuallyBoundIm Input { get; }

    protected ManuallyBoundInputModelCommand(HttpRequest request, IManuallyBoundIm input)
    {
        RequestId = Guid.NewGuid().ToString();
        Request = request;
        Input = input;
    }
}
