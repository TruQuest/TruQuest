using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

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

    public bool BindFrom(FormCollection form)
    {
        if (!(
            form.TryGetValue("type", out var value) &&
            !StringValues.IsNullOrEmpty(value) &&
            int.TryParse(value, out int type) &&
            Enum.IsDefined<SubjectTypeIm>((SubjectTypeIm)type)
        ))
        {
            return false;
        }
        Type = (SubjectTypeIm)type;

        if (!form.TryGetValue("name", out value) || StringValues.IsNullOrEmpty(value))
        {
            return false;
        }
        Name = value!;

        if (!form.TryGetValue("details", out value) || StringValues.IsNullOrEmpty(value))
        {
            return false;
        }
        Details = value!;

        if (!form.TryGetValue("file1", out value) || StringValues.IsNullOrEmpty(value))
        {
            return false;
        }
        ImagePath = value!;

        if (!form.TryGetValue("file2", out value) || StringValues.IsNullOrEmpty(value))
        {
            return false;
        }
        CroppedImagePath = value!;

        string[] valueSplit;
        if (!(
            form.TryGetValue("tags", out value) &&
            !StringValues.IsNullOrEmpty(value) &&
            (valueSplit = ((string)value!).Split('|')).Length > 0 &&
            valueSplit.All(v => int.TryParse(v, out _))
        ))
        {
            return false;
        }
        Tags = valueSplit.Select(tagId => new TagIm { Id = int.Parse(tagId) });

        return true;
    }
}
