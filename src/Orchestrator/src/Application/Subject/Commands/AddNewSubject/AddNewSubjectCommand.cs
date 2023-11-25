using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

using GoThataway;

using Domain.Errors;
using Domain.Results;
using Domain.Aggregates;
using SubjectDm = Domain.Aggregates.Subject;

using Application.Common.Interfaces;
using Application.Common.Attributes;
using Application.Common.Messages.Requests;
using Application.Common.Messages.Responses;
using Application.Common.Models.IM;

namespace Application.Subject.Commands.AddNewSubject;

[RequireAuthorization]
public class AddNewSubjectCommand : ManuallyBoundInputModelCommand, IRequest<HandleResult<Guid>>
{
    public AddNewSubjectCommand(HttpRequest request) : base(request, new NewSubjectIm()) { }
}

public class AddNewSubjectCommandHandler : IRequestHandler<AddNewSubjectCommand, HandleResult<Guid>>
{
    private readonly ILogger<AddNewSubjectCommandHandler> _logger;
    private readonly ICurrentPrincipal _currentPrincipal;
    private readonly IRequestDispatcher _requestDispatcher;
    private readonly ISubjectRepository _subjectRepository;

    public AddNewSubjectCommandHandler(
        ILogger<AddNewSubjectCommandHandler> logger,
        ICurrentPrincipal currentPrincipal,
        IRequestDispatcher requestDispatcher,
        ISubjectRepository subjectRepository
    )
    {
        _logger = logger;
        _currentPrincipal = currentPrincipal;
        _requestDispatcher = requestDispatcher;
        _subjectRepository = subjectRepository;
    }

    public async Task<HandleResult<Guid>> Handle(AddNewSubjectCommand command, CancellationToken ct)
    {
        var result = await _requestDispatcher.GetResult(new ArchiveSubjectAttachmentsCommand
        {
            SubmitterId = _currentPrincipal.Id!,
            Input = (NewSubjectIm)command.Input
        });

        if (result is ArchiveSubjectAttachmentsFailureResult failureResult)
        {
            // @@TODO: Delete files.
            return new()
            {
                Error = new HandleError(failureResult.ErrorMessage)
            };
        }

        var successResult = (ArchiveSubjectAttachmentsSuccessResult)result;

        var subject = new SubjectDm(
            name: successResult.Input.Name,
            details: successResult.Input.Details,
            type: (SubjectType)successResult.Input.Type,
            imageIpfsCid: successResult.Input.ImageIpfsCid!,
            croppedImageIpfsCid: successResult.Input.CroppedImageIpfsCid!,
            submitterId: _currentPrincipal.Id!
        );
        subject.AddTags(successResult.Input.Tags.Select(t => t.Id));

        _subjectRepository.Create(subject);
        await _subjectRepository.SaveChanges();

        return new()
        {
            Data = subject.Id!.Value
        };
    }
}
