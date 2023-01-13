namespace Attributes;

internal class WebPagePathAttribute : FilePathAttribute
{
    public required string ExtraBackingField { get; init; }
}