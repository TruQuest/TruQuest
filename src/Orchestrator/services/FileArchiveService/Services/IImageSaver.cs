using Utils;

namespace Services;

internal interface IImageSaver
{
    Task<Either<Error, string>> SaveLocalCopy(string url);
}