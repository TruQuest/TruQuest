using MediatR;

namespace Application.Common.Interfaces;

public interface IAdditionalApplicationRequestSink
{
    void Add(IBaseRequest request);
}
