using Application.Common.Models.QM;

namespace Application.Subject.Queries.GetSubject;

public class SubjectQm
{
    public Guid Id { get; }
    public string Name { get; }
    public string Details { get; }
    public int Type { get; }
    public string ImageIpfsCid { get; }
    public string CroppedImageIpfsCid { get; }
    public string SubmitterId { get; }

    public List<TagQm> Tags { get; } = new();
}