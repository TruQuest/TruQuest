using Utils;

namespace Services;

internal interface IFileArchiver
{
    Task<Error?> ArchiveAllAttachments(object input, IProgress<int> progress);
}