using Application.Dummy.Commands.CreateUser;
using Domain.Results;
using MediatR;

namespace Application.Dummy.Commands.SaveShare;

public class SaveShareCommand : IRequest<VoidResult>
{
    public required string Email { get; init; }
    public required string KeyShare { get; init; }
}

internal class SaveShareCommandHandler : IRequestHandler<SaveShareCommand, VoidResult>
{
    private readonly DummyUserRepo _dummyUserRepo;

    public SaveShareCommandHandler(DummyUserRepo dummyUserRepo)
    {
        _dummyUserRepo = dummyUserRepo;
    }

    public async Task<VoidResult> Handle(SaveShareCommand command, CancellationToken ct)
    {
        _dummyUserRepo.AddKeyShare(command.Email, command.KeyShare);
        return VoidResult.Instance;
    }
}
