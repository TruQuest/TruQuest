using Microsoft.AspNetCore.Identity;

using Domain.Base;

namespace Domain.Aggregates;

public class User : IdentityUser<string>, IAggregateRoot
{
    private List<AuthCredential> _authCredentials = new();
    public IReadOnlyList<AuthCredential> AuthCredentials => _authCredentials;

    public void AddAuthCredential(AuthCredential credential) => _authCredentials.Add(credential);
}
