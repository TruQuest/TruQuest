using Microsoft.Extensions.Logging;

using GoThataway;
using FluentValidation;

using Domain.Aggregates;
using Domain.Results;

using Application.Common.Attributes;
using Application.Common.Interfaces;
using Application.Admin.Commands.EditWhitelist.IM;
using static Application.Common.Monitoring.LogMessagePlaceholders;

namespace Application.Admin.Commands.EditWhitelist;

[RequireAuthorization(Policy = "AdminOnly")]
public class EditWhitelistCommand : IRequest<VoidResult>
{
    public ActionIm Action { get; set; }
    public required WhitelistEntryTypeIm EntryType { get; init; }
    public required string Entry { get; init; }
}

internal class Validator : AbstractValidator<EditWhitelistCommand>
{
    public Validator(IEthereumAddressFormatter addressFormatter)
    {
        RuleFor(c => c.Entry)
            .EmailAddress()
            .When(c => c.EntryType == WhitelistEntryTypeIm.Email);
        RuleFor(c => c.Entry)
            .Must(e => addressFormatter.IsValidEIP55EncodedAddress(e))
            .When(c => c.EntryType == WhitelistEntryTypeIm.SignerAddress);
    }
}

public class EditWhitelistCommandHandler : IRequestHandler<EditWhitelistCommand, VoidResult>
{
    private readonly ILogger<EditWhitelistCommandHandler> _logger;
    private readonly IWhitelistRepository _whitelistRepository;

    public EditWhitelistCommandHandler(
        ILogger<EditWhitelistCommandHandler> logger,
        IWhitelistRepository whitelistRepository
    )
    {
        _logger = logger;
        _whitelistRepository = whitelistRepository;
    }

    public async Task<VoidResult> Handle(EditWhitelistCommand command, CancellationToken ct)
    {
        if (command.Action == ActionIm.Add)
        {
            _whitelistRepository.Create(new((WhitelistEntryType)command.EntryType, command.Entry));
            await _whitelistRepository.SaveChanges();

            if (command.EntryType == WhitelistEntryTypeIm.Email)
                _logger.LogInformation($"Whitelisted user with email {Email}", command.Entry);
            else
                _logger.LogInformation($"Whitelisted user with signer address {SignerAddress}", command.Entry);
        }
        else
        {
            _whitelistRepository.Remove(new((WhitelistEntryType)command.EntryType, command.Entry));
            await _whitelistRepository.SaveChanges();

            if (command.EntryType == WhitelistEntryTypeIm.Email)
                _logger.LogInformation($"Removed user with email {Email} from whitelist", command.Entry);
            else
                _logger.LogInformation($"Removed user with signer address {SignerAddress} from whitelist", command.Entry);
        }

        return VoidResult.Instance;
    }
}
