using EduVote.Application.Common;
using EduVote.DAL.Postgresql.Repositories.Interfaces;
using MediatR;

namespace EduVote.Application.Votings.GetVoting;

public sealed class GetVotingQueryHandler(IVotingRepository votingRepository)
    : IRequestHandler<GetVotingQuery, VotingDetails>
{
    public async Task<VotingDetails> Handle(GetVotingQuery request, CancellationToken cancellationToken)
    {
        var voting = await votingRepository.GetByIdAsync(request.VotingId, cancellationToken);

        if (voting is null)
        {
            throw new ApplicationErrorException(
                ApplicationErrorType.NotFound,
                $"Voting with id {request.VotingId} was not found.");
        }

        return voting.ToDetails();
    }
}
