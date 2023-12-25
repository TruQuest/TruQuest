using Microsoft.Extensions.Logging;

using GoThataway;
using FluentValidation;

using Domain.Errors;
using Domain.Results;

using Application.Common.Attributes;
using Application.Common.Interfaces;
using static Application.Common.Monitoring.LogMessagePlaceholders;

namespace Application.Admin.Commands.GiveOrRemoveRestrictedAccess;

[RequireAuthorization(Policy = "AdminOnly")]
public class GiveOrRemoveRestrictedAccessCommand : IRequest<HandleResult<string>>
{
    public ActionIm Action { get; set; }
    public required IEnumerable<string> Addresses { get; init; }
}

internal class Validator : AbstractValidator<GiveOrRemoveRestrictedAccessCommand>
{
    public Validator(IEthereumAddressFormatter addressFormatter)
    {
        RuleFor(c => c.Addresses)
            .NotEmpty()
            .ForEach((ap) => ap.Must(a => addressFormatter.IsValidEIP55EncodedAddress(a)));
        RuleFor(c => c.Addresses)
            .Must(addresses => addresses.Count() == 1)
            .When(c => c.Action == ActionIm.Remove);
    }
}

public class GiveOrRemoveRestrictedAccessCommandHandler : IRequestHandler<GiveOrRemoveRestrictedAccessCommand, HandleResult<string>>
{
    private readonly ILogger<GiveOrRemoveRestrictedAccessCommandHandler> _logger;
    private readonly IContractCaller _contractCaller;

    public GiveOrRemoveRestrictedAccessCommandHandler(
        ILogger<GiveOrRemoveRestrictedAccessCommandHandler> logger,
        IContractCaller contractCaller
    )
    {
        _logger = logger;
        _contractCaller = contractCaller;
    }

    public async Task<HandleResult<string>> Handle(GiveOrRemoveRestrictedAccessCommand command, CancellationToken ct)
    {
        String? txnHash;
        if (command.Action == ActionIm.Give)
        {
            List<string>? alreadyHaveAccess = null;
            foreach (var address in command.Addresses)
            {
                bool hasAccess = await _contractCaller.CheckHasAccess(address);
                if (hasAccess)
                {
                    alreadyHaveAccess ??= new List<string>();
                    alreadyHaveAccess.Add(address);
                }
            }

            var addressesToGiveAccessTo = command.Addresses;
            if (alreadyHaveAccess != null)
            {
                _logger.LogWarning($"These addresses already have restricted access: {string.Join(", ", alreadyHaveAccess)}");
                addressesToGiveAccessTo = addressesToGiveAccessTo.Except(alreadyHaveAccess).ToList();
            }

            if (addressesToGiveAccessTo.Count() == 1)
                txnHash = await _contractCaller.GiveAccessTo(addressesToGiveAccessTo.Single());
            else if (addressesToGiveAccessTo.Count() > 1)
                txnHash = await _contractCaller.GiveAccessToMany(addressesToGiveAccessTo.ToList());
            else
            {
                _logger.LogWarning("All requested addresses already have restricted access");
                return new()
                {
                    Error = new HandleError("All requested addresses already have restricted access")
                };
            }
        }
        else
        {
            var address = command.Addresses.Single();
            var index = await _contractCaller.GetIndexOfWhitelistedUser(address);
            if (index < 0)
            {
                _logger.LogWarning($"{WalletAddress} doesn't have restricted access", address);
                return new()
                {
                    Error = new HandleError($"{address} doesn't have restricted access")
                };
            }

            txnHash = await _contractCaller.RemoveAccessFrom(address, (ushort)index);
        }

        if (txnHash == null)
        {
            return new()
            {
                Error = new HandleError($"Error trying to {command.Action.ToString().ToLower()} restricted access")
            };
        }

        return new()
        {
            Data = txnHash!
        };
    }
}
