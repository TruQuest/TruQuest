using Microsoft.AspNetCore.Http;

namespace Application.Common.Interfaces;

public interface IManuallyBoundIm
{
    bool BindFrom(FormCollection form);
}
