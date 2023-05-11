using Domain.Aggregates;

namespace Application.Common.Interfaces;

public interface IClientNotifier
{
    Task TellAboutNewThingDraftCreationProgress(string userId, Guid thingId, int percent);
    Task TellThatNewThingDraftCreatedSuccessfully(string userId, Guid thingId);
    Task SubscribeToThing(string connectionId, Guid thingId);
    Task UnsubscribeFromThing(string connectionId, Guid thingId);
    Task NotifyThingStateChanged(Guid thingId, ThingState state);
    Task SubscribeToSettlementProposal(string connectionId, Guid proposalId);
    Task UnsubscribeFromSettlementProposal(string connectionId, Guid proposalId);
    Task TellAboutNewSettlementProposalDraftCreationProgress(string userId, Guid proposalId, int percent);
    Task NotifySettlementProposalStateChanged(Guid proposalId, SettlementProposalState state);
    Task NotifyUsersAboutItemUpdate(
        IEnumerable<string> userIds, long updateTimestamp, WatchedItemType itemType,
        Guid itemId, string title, string? details
    );
}