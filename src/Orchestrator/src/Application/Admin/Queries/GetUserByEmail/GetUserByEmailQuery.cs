using GoThataway;
using FluentValidation;

using Domain.Aggregates;
using Domain.Errors;
using Domain.Results;

using Application.Common.Attributes;

namespace Application.Admin.Queries.GetUserByEmail;

[RequireAuthorization(Policy = "AdminOnly")]
public class GetUserByEmailQuery : IRequest<HandleResult<GetUserByEmailRvm>>
{
    public required string Email { get; init; }
}

internal class Validator : AbstractValidator<GetUserByEmailQuery>
{
    public Validator()
    {
        RuleFor(q => q.Email).EmailAddress();
    }
}

public class GetUserByEmailQueryHandler : IRequestHandler<GetUserByEmailQuery, HandleResult<GetUserByEmailRvm>>
{
    private readonly IUserRepository _userRepository;

    public GetUserByEmailQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<HandleResult<GetUserByEmailRvm>> Handle(GetUserByEmailQuery query, CancellationToken ct)
    {
        // @@TODO: Use queryable instead of repo.
        var user = await _userRepository.FindByEmail(query.Email);
        if (user == null)
        {
            return new()
            {
                Error = new HandleError("Not found")
            };
        }

        return new()
        {
            Data = new()
            {
                UserId = user.Id,
                WalletAddress = user.WalletAddress,
                SignerAddress = user.UserName!,
                EmailConfirmed = user.EmailConfirmed
            }
        };
    }
}
