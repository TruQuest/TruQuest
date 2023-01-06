using Microsoft.Extensions.Logging;

using MediatR;

using Domain.Results;
using Domain.Aggregates;
using SubjectDm = Domain.Aggregates.Subject;

using Application.Common.Interfaces;
using Application.Common.Attributes;

namespace Application.Subject.Commands.AddNewSubject;

[RequireAuthorization]
public class AddNewSubjectCommand : IRequest<HandleResult<Guid>>
{
    public NewSubjectIm Input { get; set; }
    public string Signature { get; set; }
}

internal class AddNewSubjectCommandHandler : IRequestHandler<AddNewSubjectCommand, HandleResult<Guid>>
{
    private readonly ILogger<AddNewSubjectCommandHandler> _logger;
    private readonly ICurrentPrincipal _currentPrincipal;
    private readonly ISigner _signer;
    private readonly IFileArchiver _fileArchiver;
    private readonly ISubjectRepository _subjectRepository;

    public AddNewSubjectCommandHandler(
        ILogger<AddNewSubjectCommandHandler> logger,
        ICurrentPrincipal currentPrincipal,
        ISigner signer,
        IFileArchiver fileArchiver,
        ISubjectRepository subjectRepository
    )
    {
        _logger = logger;
        _currentPrincipal = currentPrincipal;
        _signer = signer;
        _fileArchiver = fileArchiver;
        _subjectRepository = subjectRepository;
    }

    public async Task<HandleResult<Guid>> Handle(AddNewSubjectCommand command, CancellationToken ct)
    {
        var result = _signer.RecoverFromNewSubjectMessage(command.Input, command.Signature);
        if (result.IsError)
        {
            return new()
            {
                Error = result.Error
            };
        }

        // check that result.Data == _currentPrincipal.Id

        await _fileArchiver.ArchiveAll(command.Input);

        var subject = new SubjectDm(
            name: command.Input.Name,
            details: command.Input.Details,
            type: (int)command.Input.Type,
            imageIpfsCid: command.Input.ImageIpfsCid,
            submitterId: _currentPrincipal.Id
        );
        subject.AddTags(command.Input.Tags.Select(t => t.Id));

        _subjectRepository.Create(subject);
        await _subjectRepository.SaveChanges();

        return new()
        {
            Data = subject.Id!.Value
        };
    }
}