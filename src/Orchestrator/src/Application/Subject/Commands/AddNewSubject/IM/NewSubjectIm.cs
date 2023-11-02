using Microsoft.AspNetCore.Http;

using Application.Common.Interfaces;
using Application.Common.Models.IM;

namespace Application.Subject.Commands.AddNewSubject;

public class NewSubjectIm : IManuallyBoundIm
{
    public SubjectTypeIm Type { get; set; } // @@NOTE: 'required' + 'init' works, 'set' works, but 'private set' doesn't work.
    public string Name { get; set; }
    public string Details { get; set; }
    public string ImagePath { get; set; }
    public string CroppedImagePath { get; set; }
    public IEnumerable<TagIm> Tags { get; set; }

    public string? ImageIpfsCid { get; set; }
    public string? CroppedImageIpfsCid { get; set; }

    public void BindFrom(FormCollection form)
    {
        // @@TODO: Validate.
        Type = (SubjectTypeIm)int.Parse(form["type"]!);
        Name = form["name"]!;
        Details = form["details"]!;
        ImagePath = form["file1"]!;
        CroppedImagePath = form["file2"]!;
        Tags = ((string)form["tags"]!).Split('|')
            .Select(tagIdStr => new TagIm { Id = int.Parse(tagIdStr) });
    }
}
