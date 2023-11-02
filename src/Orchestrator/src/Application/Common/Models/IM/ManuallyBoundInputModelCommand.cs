using Microsoft.AspNetCore.Http;

using Application.Common.Interfaces;

namespace Application.Common.Models.IM;

public abstract class ManuallyBoundInputModelCommand
{
    public HttpRequest Request { get; }
    public IManuallyBoundIm Input { get; }

    protected ManuallyBoundInputModelCommand(HttpRequest request, IManuallyBoundIm input)
    {
        Request = request;
        Input = input;
    }
}
