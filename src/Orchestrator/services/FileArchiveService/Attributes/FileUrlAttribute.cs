namespace Attributes;

[AttributeUsage(AttributeTargets.Property)]
internal abstract class FileUrlAttribute : Attribute
{
    public required string BackingField { get; init; }
}