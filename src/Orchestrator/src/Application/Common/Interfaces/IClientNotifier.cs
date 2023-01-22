using Domain.Aggregates;

namespace Application.Common.Interfaces;

public interface IClientNotifier
{
    Task TellAboutNewThingDraftCreationProgress(string userId, Guid thingId, int percent);
    Task TellThatNewThingDraftCreatedSuccessfully(string userId, Guid thingId);
    Task SubscribeToThing(string connectionId, Guid thingId);
    Task UnsubscribeFromThing(string connectionId, Guid thingId);
    Task NotifyThingStateChanged(Guid thingId, ThingState state);
}