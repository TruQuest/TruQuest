namespace API.Hubs.Clients;

public interface ITruQuestClient
{
    Task TellAboutNewThingDraftCreationProgress(string thingId, int percent);
}