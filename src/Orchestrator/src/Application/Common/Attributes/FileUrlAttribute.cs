namespace Application.Common.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public abstract class FileUrlAttribute : Attribute
{
    public required string BackingField { get; init; }
}