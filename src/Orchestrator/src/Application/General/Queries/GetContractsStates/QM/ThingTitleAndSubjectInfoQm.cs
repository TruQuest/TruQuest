namespace Application.General.Queries.GetContractsStates.QM;

public class ThingTitleAndSubjectInfoQm
{
    public required Guid Id { get; init; }
    public required string Title { get; init; }
    public required Guid SubjectId { get; init; }
    public required string SubjectName { get; init; }
}
