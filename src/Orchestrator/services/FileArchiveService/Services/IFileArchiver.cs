using Utils;

namespace Services;

internal interface IFileArchiver
{
    Task<Error?> ArchiveAllAttachments(string requestId, object input, IProgress<int>? progress = null);
}
