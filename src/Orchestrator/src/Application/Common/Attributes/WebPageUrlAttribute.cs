namespace Application.Common.Attributes;

public class WebPageUrlAttribute : FileUrlAttribute
{
    public required string ExtraBackingField { get; init; }
}