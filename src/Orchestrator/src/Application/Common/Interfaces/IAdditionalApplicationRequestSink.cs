using MediatR;

namespace Application.Common.Interfaces;

public interface IAdditionalApplicationRequestSink
{
    ValueTask Add(IBaseRequest request);
}
