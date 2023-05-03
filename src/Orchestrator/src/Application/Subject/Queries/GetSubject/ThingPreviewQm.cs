using Application.Common.Models.QM;

namespace Application.Subject.Queries.GetSubject;

public class ThingPreviewQm
{
    public Guid Id { get; }
    public ThingStateQm State { get; }
    public string Title { get; }
    public string? CroppedImageIpfsCid { get; }
    public long SortedByDate { get; }
}