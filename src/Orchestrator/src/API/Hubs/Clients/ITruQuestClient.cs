namespace API.Hubs.Clients;

public interface ITruQuestClient
{
    Task TellAboutNewThingDraftCreationProgress(string thingId, int percent);
    Task NotifyThingStateChanged(string thingId, int state);
    Task TellAboutNewSettlementProposalDraftCreationProgress(string proposalId, int percent);
    Task NotifySettlementProposalStateChanged(string proposalId, int state);
}