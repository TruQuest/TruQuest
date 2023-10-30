using FluentValidation;

namespace Application.Thing.Common.Models.IM;

public class ThingEvidenceIm
{
    public required string Url { get; init; }
    public string? IpfsCid { get; init; }
    public string? PreviewImageIpfsCid { get; init; }
}

internal class ThingEvidenceImValidator : AbstractValidator<ThingEvidenceIm>
{
    public ThingEvidenceImValidator()
    {
        RuleFor(e => e.Url).Must(_beAValidUrl);
        RuleFor(e => e.IpfsCid).Null();
        RuleFor(e => e.PreviewImageIpfsCid).Null();
    }

    private bool _beAValidUrl(string url) =>
        Uri.TryCreate(url, UriKind.Absolute, out Uri? validatedUri) &&
        (validatedUri.Scheme == Uri.UriSchemeHttp || validatedUri.Scheme == Uri.UriSchemeHttps);
}
