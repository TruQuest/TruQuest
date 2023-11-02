using Microsoft.AspNetCore.Http;

namespace Application.Common.Interfaces;

public interface IManuallyBoundIm
{
    void BindFrom(FormCollection form);
}
