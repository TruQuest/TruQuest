namespace Application.Common.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public class RequireAuthorizationAttribute : Attribute
{
    public string? Policy { get; init; }
    public string? Roles { get; init; }
}