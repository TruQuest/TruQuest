namespace Application.Common.Interfaces;

public interface ICurrentPrincipal
{
    string? Id { get; }
    string? Username { get; }
}