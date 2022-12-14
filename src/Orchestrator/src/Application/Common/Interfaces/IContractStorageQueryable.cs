namespace Application.Common.Interfaces;

public interface IContractStorageQueryable
{
    Task<string> GetVerifierLotteryParticipantAt(string thingId, int index);
}