using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;

using Nethereum.Util;

using Domain.Aggregates;

namespace Infrastructure.Persistence;

public class KeccakSha3Generator : ValueGenerator<string>
{
    public override bool GeneratesTemporaryValues => false;

    public override string Next(EntityEntry entry)
    {
        var id = (Guid)entry.Property(nameof(Thing.Id)).CurrentValue!;
        return Sha3Keccack.Current.CalculateHash(id.ToString());
    }
}