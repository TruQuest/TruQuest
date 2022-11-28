namespace Application.Common.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public abstract class FileURLAttribute : Attribute
{
    public bool KeepOriginURL { get; set; } = false;
}