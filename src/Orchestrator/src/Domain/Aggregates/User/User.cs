using Microsoft.AspNetCore.Identity;

using Domain.Base;

namespace Domain.Aggregates;

public class User : IdentityUser<string>, IAggregateRoot { }