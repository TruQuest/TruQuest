namespace Application.Common.Interfaces;

public interface IKeccakHasher
{
    byte[] Hash(params string[] values);
}
