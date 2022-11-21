namespace Domain.Aggregates;

public class SubjectAttachedTag
{
    public Guid? SubjectId { get; private set; }
    public int TagId { get; }

    internal SubjectAttachedTag(int tagId)
    {
        TagId = tagId;
    }
}