namespace Attributes;

internal class WebPageUrlAttribute : FileUrlAttribute
{
    public required string ExtraBackingField { get; init; }
}