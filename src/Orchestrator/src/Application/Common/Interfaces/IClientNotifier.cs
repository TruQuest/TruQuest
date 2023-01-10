namespace Application.Common.Interfaces;

public interface IClientNotifier
{
    Task TellAboutNewThingDraftCreationProgress(string userId, Guid thingId, int percent);
    Task TellThatNewThingDraftCreatedSuccessfully(string userId, Guid thingId);
}