namespace Application.Common.Interfaces;

public interface IContractCaller
{
    Task<byte[]> ComputeHash(byte[] data);
    Task InitVerifierLottery(string thingId, byte[] dataHash);
}