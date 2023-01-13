namespace Attributes;

[AttributeUsage(AttributeTargets.Property)]
internal abstract class FilePathAttribute : Attribute
{
    public required string BackingField { get; init; }
}