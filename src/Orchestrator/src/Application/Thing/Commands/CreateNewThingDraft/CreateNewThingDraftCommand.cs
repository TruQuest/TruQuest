using Microsoft.AspNetCore.Http;

using MediatR;

using Domain.Results;

using Application.Thing.Common.Models.IM;
using Application.Common.Attributes;
using Application.Common.Interfaces;
using Application.Common.Messages.Requests;
using Application.Common.Models.IM;

namespace Application.Thing.Commands.CreateNewThingDraft;

// [RequireAuthorization]
public class CreateNewThingDraftCommand : IRequest<HandleResult<Guid>>
{
    public required HttpRequest Request { get; init; }
}

internal class CreateNewThingDraftCommandHandler : IRequestHandler<CreateNewThingDraftCommand, HandleResult<Guid>>
{
    // private readonly ICurrentPrincipal _currentPrincipal;
    private readonly IFileReceiver _fileReceiver;
    private readonly IRequestDispatcher _requestDispatcher;

    public CreateNewThingDraftCommandHandler(
        // ICurrentPrincipal currentPrincipal,
        IFileReceiver fileReceiver,
        IRequestDispatcher requestDispatcher
    )
    {
        // _currentPrincipal = currentPrincipal;
        _fileReceiver = fileReceiver;
        _requestDispatcher = requestDispatcher;
    }

    public async Task<HandleResult<Guid>> Handle(CreateNewThingDraftCommand command, CancellationToken ct)
    {
        var result = await _fileReceiver.ReceiveFilesAndFormValues(
            command.Request,
            maxSize: 10 * 1024 * 1024,
            filePrefix: "bF2Ff171C3C4A63FBBD369ddb021c75934005e81".ToLower()
        );
        if (result.IsError)
        {
            return new()
            {
                Error = result.Error
            };
        }

        var formValues = result.Data!;
        // @@TODO: Validate form.

        var input = new NewThingIm
        {
            SubjectId = Guid.Parse(formValues["subjectId"]!),
            Title = formValues["title"]!,
            Details = formValues["details"]!,
            ImagePath = formValues["file1"],
            CroppedImagePath = formValues["file2"],
            Evidence = ((string)formValues["evidence"]!).Split('|')
                .Select(url => new EvidenceIm
                {
                    Url = url
                })
                .ToList(),
            Tags = ((string)formValues["tags"]!).Split('|')
                .Select(tagIdStr => new TagIm
                {
                    Id = int.Parse(tagIdStr)
                })
                .ToList()
        };

        var thingId = Guid.NewGuid();

        await _requestDispatcher.Send(new ArchiveThingAttachmentsCommand
        {
            SubmitterId = "bF2Ff171C3C4A63FBBD369ddb021c75934005e81".ToLower(),
            ThingId = thingId,
            Input = input
        });

        return new()
        {
            Data = thingId
        };
    }
}