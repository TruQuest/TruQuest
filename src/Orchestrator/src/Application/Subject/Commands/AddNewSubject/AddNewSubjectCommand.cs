using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

using MediatR;

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
public class AddNewSubjectCommand : IRequest<HandleResult<Guid>>
{
    public required HttpRequest Request { get; init; }
}

internal class AddNewSubjectCommandHandler : IRequestHandler<AddNewSubjectCommand, HandleResult<Guid>>
{
    private readonly ILogger<AddNewSubjectCommandHandler> _logger;
    private readonly ICurrentPrincipal _currentPrincipal;
    private readonly IFileReceiver _fileReceiver;
    private readonly IRequestDispatcher _requestDispatcher;
    private readonly ISubjectRepository _subjectRepository;

    public AddNewSubjectCommandHandler(
        ILogger<AddNewSubjectCommandHandler> logger,
        ICurrentPrincipal currentPrincipal,
        IFileReceiver fileReceiver,
        IRequestDispatcher requestDispatcher,
        ISubjectRepository subjectRepository
    )
    {
        _logger = logger;
        _currentPrincipal = currentPrincipal;
        _fileReceiver = fileReceiver;
        _requestDispatcher = requestDispatcher;
        _subjectRepository = subjectRepository;
    }

    public async Task<HandleResult<Guid>> Handle(AddNewSubjectCommand command, CancellationToken ct)
    {
        var receiveResult = await _fileReceiver.ReceiveFilesAndFormValues(
            command.Request,
            maxSize: 10 * 1024 * 1024,
            filePrefix: _currentPrincipal.Id!
        );
        if (receiveResult.IsError)
        {
            return new()
            {
                Error = receiveResult.Error
            };
        }

        var formValues = receiveResult.Data!;
        // @@TODO: Validate form.

        var input = new NewSubjectIm
        {
            Type = (SubjectTypeIm)int.Parse(formValues["type"]!),
            Name = formValues["name"]!,
            Details = formValues["details"]!,
            ImagePath = formValues["file1"]!,
            CroppedImagePath = formValues["file2"]!,
            Tags = ((string)formValues["tags"]!).Split('|')
                .Select(tagIdStr => new TagIm
                {
                    Id = int.Parse(tagIdStr)
                })
                .ToList()
        };

        var result = await _requestDispatcher.GetResult(new ArchiveSubjectAttachmentsCommand
        {
            SubmitterId = _currentPrincipal.Id!,
            Input = input
        });

        if (result is ArchiveSubjectAttachmentsFailureResult failureResult)
        {
            // @@TODO: Delete files.
            return new()
            {
                Error = new SubjectError(failureResult.ErrorMessage)
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