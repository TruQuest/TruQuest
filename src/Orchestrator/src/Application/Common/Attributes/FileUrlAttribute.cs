namespace Application.Common.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public abstract class FileUrlAttribute : Attribute
{
    public bool KeepOriginUrl { get; set; } = false;
}