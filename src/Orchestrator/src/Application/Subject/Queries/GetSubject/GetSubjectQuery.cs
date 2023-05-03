using MediatR;

using Domain.Results;
using Domain.Errors;

using Application.Common.Interfaces;

namespace Application.Subject.Queries.GetSubject;

public class GetSubjectQuery : IRequest<HandleResult<SubjectQm>>
{
    public required Guid Id { get; init; }
}

internal class GetSubjectQueryHandler : IRequestHandler<GetSubjectQuery, HandleResult<SubjectQm>>
{
    private readonly ISubjectQueryable _subjectQueryable;

    public GetSubjectQueryHandler(ISubjectQueryable subjectQueryable)
    {
        _subjectQueryable = subjectQueryable;
    }

    public async Task<HandleResult<SubjectQm>> Handle(GetSubjectQuery query, CancellationToken ct)
    {
        var subject = await _subjectQueryable.GetById(query.Id);
        if (subject == null)
        {
            return new()
            {
                Error = new SubjectError("Not found")
            };
        }

        return new()
        {
            Data = subject
        };
    }
}