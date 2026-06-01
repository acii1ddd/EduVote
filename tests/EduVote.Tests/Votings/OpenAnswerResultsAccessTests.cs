using EduVote.Application.Votings.GetResults;
using EduVote.DAL.Postgresql.Models.Roles;

namespace EduVote.Tests.Votings;

public class OpenAnswerResultsAccessTests
{
    private static readonly Guid OrganizerId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");
    private static readonly Guid OtherUserId = Guid.Parse("11111111-2222-3333-4444-555555555555");

    [Fact]
    public void CanViewAnswerTexts_Administrator_Always_Allowed()
    {
        Assert.True(OpenAnswerResultsAccess.CanViewAnswerTexts(Roles.Administrator, null, OrganizerId));
        Assert.True(OpenAnswerResultsAccess.CanViewAnswerTexts(Roles.Administrator, OtherUserId, OrganizerId));
    }

    [Fact]
    public void CanViewAnswerTexts_Teacher_Only_When_Organizer()
    {
        Assert.True(OpenAnswerResultsAccess.CanViewAnswerTexts(Roles.Teacher, OrganizerId, OrganizerId));
        Assert.False(OpenAnswerResultsAccess.CanViewAnswerTexts(Roles.Teacher, OtherUserId, OrganizerId));
    }

    [Fact]
    public void CanViewAnswerTexts_Student_Never_Allowed()
    {
        Assert.False(OpenAnswerResultsAccess.CanViewAnswerTexts(Roles.Student, OrganizerId, OrganizerId));
        Assert.False(OpenAnswerResultsAccess.CanViewAnswerTexts(Roles.Student, OtherUserId, OrganizerId));
    }

    [Fact]
    public void CanViewAnswerTexts_Without_Role_Never_Allowed()
    {
        Assert.False(OpenAnswerResultsAccess.CanViewAnswerTexts(null, OrganizerId, OrganizerId));
    }

    [Fact]
    public void RedactAnswerTexts_Clears_Answers_Keeps_Total()
    {
        const string resultData =
            """{"openAnswer":{"totalAnswers":2,"answers":["first","second"]}}""";

        var redacted = OpenAnswerResultDataRedactor.RedactAnswerTexts(resultData);

        Assert.Contains("\"totalAnswers\":2", redacted);
        Assert.Contains("\"answers\":[]", redacted);
        Assert.DoesNotContain("first", redacted);
        Assert.DoesNotContain("second", redacted);
    }
}
