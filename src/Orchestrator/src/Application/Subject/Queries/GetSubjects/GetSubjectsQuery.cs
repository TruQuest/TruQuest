using MediatR;

using Domain.Results;

using Application.Common.Interfaces;

namespace Application.Subject.Queries.GetSubjects;

public class GetSubjectsQuery : IRequest<HandleResult<IEnumerable<SubjectPreviewQm>>> { }

internal class GetSubjectsQueryHandler :
    IRequestHandler<GetSubjectsQuery, HandleResult<IEnumerable<SubjectPreviewQm>>>
{
    private readonly ISubjectQueryable _subjectQueryable;

    public GetSubjectsQueryHandler(ISubjectQueryable subjectQueryable)
    {
        _subjectQueryable = subjectQueryable;
    }

    public async Task<HandleResult<IEnumerable<SubjectPreviewQm>>> Handle(
        GetSubjectsQuery query, CancellationToken ct
    )
    {
        var subjects = await _subjectQueryable.GetAll();
        return new()
        {
            Data = subjects
        };
    }
}