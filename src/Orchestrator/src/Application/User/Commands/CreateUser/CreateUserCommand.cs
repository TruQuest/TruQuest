using MediatR;

using Domain.Aggregates;
using Domain.Results;
using UserDm = Domain.Aggregates.User;

namespace Application.User.Commands.CreateUser;

public class CreateUserCommand : IRequest<VoidResult>
{
    public required string Email { get; init; }
}

internal class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, VoidResult>
{
    private readonly IUserRepository _userRepository;

    public CreateUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<VoidResult> Handle(CreateUserCommand command, CancellationToken ct)
    {
        var userId = Guid.NewGuid().ToString();
        var user = new UserDm
        {
            Id = userId,
            Email = command.Email,
            UserName = command.Email
        };

        var error = await _userRepository.Create(user);
        if (error != null)
        {
            return new()
            {
                Error = error
            };
        }

        return VoidResult.Instance;
    }
}
