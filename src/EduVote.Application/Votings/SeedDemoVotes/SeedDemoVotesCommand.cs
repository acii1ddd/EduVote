using MediatR;

namespace EduVote.Application.Votings.SeedDemoVotes;

public sealed record SeedDemoVotesCommand(string VotingTitleSubstring, int VoteCount)
    : IRequest<SeedDemoVotesResult>;
