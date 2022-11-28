namespace Domain.Aggregates;

public class ThingAttachedTag
{
    public Guid? ThingId { get; private set; }
    public int TagId { get; }

    internal ThingAttachedTag(int tagId)
    {
        TagId = tagId;
    }
}