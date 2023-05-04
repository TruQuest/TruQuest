using Application.Common.Models.QM;

namespace Application.Subject.Common.Models.QM;

public class ThingPreviewQm
{
    public Guid Id { get; }
    public ThingStateQm State { get; }
    public string Title { get; }
    public string? CroppedImageIpfsCid { get; }
    public long? DisplayedTimestamp { get; }
    public VerdictQm? Verdict { get; }
}