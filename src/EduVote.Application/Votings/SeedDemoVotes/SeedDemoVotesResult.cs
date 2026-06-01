namespace EduVote.Application.Votings.SeedDemoVotes;

public sealed record SeedDemoVotesResult(
    Guid VotingId,
    string VotingTitle,
    int VoteCount,
    int DemoUsersCreated);
