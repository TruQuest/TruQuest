using Domain.Results;

using Domain.Errors;

namespace Application.Common.Interfaces;

public interface IFileStorage
{
    Task<Either<HandleError, string>> UploadJson(object obj);
}
