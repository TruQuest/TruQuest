using Application.Common.Models.IM;
using Application.Subject.Commands.AddNewSubject;

namespace Tests.FunctionalTests;

[Collection(nameof(TruQuestTestCollection))]
public class VerifierLottery
{
    private readonly Sut _sut;

    public VerifierLottery(Sut sut)
    {
        _sut = sut;
        _sut.ResetState();
    }

    [Fact]
    public async Task ShouldDoDo()
    {
        var input = new NewSubjectIm
        {
            Name = "Name",
            Details = "Details",
            Type = SubjectTypeIm.Person,
            ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/b/b6/Image_created_with_a_mobile_phone.png/640px-Image_created_with_a_mobile_phone.png",
            Tags = new List<TagIm>() { new() { Id = 1 } }
        };

        var sig = _sut.Signer.SignNewSubjectMessage(input);

        _sut.RunAs(userId: "bF2Ff171C3C4A63FBBD369ddb021c75934005e81", username: "player");

        var result = await _sut.SendRequest(new AddNewSubjectCommand
        {
            Input = input,
            Signature = sig
        });

        var subjectId = result.Data;
    }
}