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
using Application.Common.Monitoring;
using static Application.Common.Monitoring.LogMessagePlaceholders;

namespace Application.Subject.Commands.AddNewSubject;

[RequireAuthorization]
public class AddNewSubjectCommand : ManuallyBoundInputModelCommand, IRequest<HandleResult<Guid>>
{
    public AddNewSubjectCommand(HttpRequest request) : base(request, new NewSubjectIm()) { }

    public IEnumerable<(string Name, object? Value)> GetActivityTags(HandleResult<Guid> response)
    {
        var tags = new List<(string, object?)>();
        if (response.Error == null) tags.Add((ActivityTags.SubjectId, response.Data));
        return tags;
    }
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
        var result = await _requestDispatcher.GetResult(
            new ArchiveSubjectAttachmentsCommand
            {
                SubmitterId = _currentPrincipal.Id!,
                Input = (NewSubjectIm)command.Input
            },
            requestId: command.RequestId
        );

        if (result is ArchiveSubjectAttachmentsFailureResult failureResult)
        {
            _logger.LogWarning(
                $"Error trying to archive attachments for subject {SubjectName}: {failureResult.ErrorMessage}",
                ((NewSubjectIm)command.Input).Name
            );
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

        _logger.LogInformation($"Subject (Id: {SubjectId}, Name: {SubjectName}) created", subject.Id!.Value, subject.Name);

        return new()
        {
            Data = subject.Id!.Value
        };
    }
}
