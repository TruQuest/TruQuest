using GoThataway;

using Domain.Results;

using Application.Common.Interfaces;

namespace Application.General.Queries.GetFrontendEnvFile;

public class GetFrontendEnvFileQuery : IRequest<HandleResult<string>> { }

public class GetFrontendEnvFileQueryHandler : IRequestHandler<GetFrontendEnvFileQuery, HandleResult<string>>
{
    private readonly IFrontendEnvFileProvider _frontendEnvFileProvider;

    public GetFrontendEnvFileQueryHandler(IFrontendEnvFileProvider frontendEnvFileProvider)
    {
        _frontendEnvFileProvider = frontendEnvFileProvider;
    }

    public Task<HandleResult<string>> Handle(GetFrontendEnvFileQuery query, CancellationToken ct)
    {
        return Task.FromResult(new HandleResult<string>
        {
            Data = _frontendEnvFileProvider.GetContents()
        });
    }
}
