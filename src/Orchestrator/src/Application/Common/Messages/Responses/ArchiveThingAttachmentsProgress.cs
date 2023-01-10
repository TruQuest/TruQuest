namespace Application.Common.Messages.Responses;

public class ArchiveThingAttachmentsProgress
{
    public string SubmitterId { get; set; }
    public Guid ThingId { get; set; }
    public int Percent { get; set; }
}