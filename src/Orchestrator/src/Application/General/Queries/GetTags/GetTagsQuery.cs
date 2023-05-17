using MediatR;

using Domain.Results;

using Application.Common.Models.QM;
using Application.Common.Interfaces;

namespace Application.General.Queries.GetTags;

public class GetTagsQuery : IRequest<HandleResult<IEnumerable<TagQm>>> { }

internal class GetTagsQueryHandler : IRequestHandler<GetTagsQuery, HandleResult<IEnumerable<TagQm>>>
{
    private readonly ITagQueryable _tagQueryable;

    public GetTagsQueryHandler(ITagQueryable tagQueryable)
    {
        _tagQueryable = tagQueryable;
    }

    public async Task<HandleResult<IEnumerable<TagQm>>> Handle(GetTagsQuery query, CancellationToken ct)
    {
        var tags = await _tagQueryable.GetAll();
        return new()
        {
            Data = tags
        };
    }
}