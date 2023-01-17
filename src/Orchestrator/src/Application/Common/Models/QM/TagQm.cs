namespace Application.Common.Models.QM;

public class TagQm
{
    public int Id { get; }
    public string Name { get; }

    public override bool Equals(object? obj)
    {
        var other = obj as TagQm;
        if (other == null) return false;
        return Id == other.Id;
    }

    public override int GetHashCode() => Id.GetHashCode();
}