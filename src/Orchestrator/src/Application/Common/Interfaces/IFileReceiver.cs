using Microsoft.AspNetCore.Http;

using Domain.Errors;
using Domain.Results;

namespace Application.Common.Interfaces;

public interface IFileReceiver
{
    Task<Either<HandleError, FormCollection>> ReceiveFilesAndFormValues(
        HttpRequest request, int maxSize, string filePrefix
    );
}
