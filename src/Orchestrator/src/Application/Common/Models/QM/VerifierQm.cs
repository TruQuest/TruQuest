namespace Application.Common.Models.QM;

public class VerifierQm
{
    public string VerifierId { get; }
    public string UserName { get; }
    public VoteQm? Vote { get; set; }
}