using EduVote.Application.Common;
using EduVote.Application.Votings.CreateVoting;
using EduVote.DAL.Postgresql.Models;
using EduVote.DAL.Postgresql.Repositories.Interfaces;
using MediatR;

namespace EduVote.Application.Votings.UpdateVoting;

public sealed class UpdateVotingCommandHandler(IVotingRepository votingRepository)
    : IRequestHandler<UpdateVotingCommand, UpdateVotingResult>
{
    public async Task<UpdateVotingResult> Handle(
        UpdateVotingCommand request,
        CancellationToken cancellationToken)
    {
        CreateVotingValidator.ValidateDateRange(request.StartTime, request.EndTime);

        var voting = await votingRepository.GetByIdAsync(request.Id, cancellationToken);

        if (voting is null)
        {
            throw new ApplicationErrorException(
                ApplicationErrorType.NotFound,
                $"Voting with id {request.Id} was not found.");
        }

        voting.Title = request.Title;
        voting.Description = request.Description;
        voting.Type = request.Type;
        voting.IsAnonymous = request.IsAnonymous;
        voting.AllowVoteChange = request.AllowVoteChange;
        voting.StartTime = request.StartTime.ToUniversalTime();
        voting.EndTime = request.EndTime.ToUniversalTime();

        await votingRepository.SaveChangesAsync(cancellationToken);

        return voting.ToUpdateVotingResult();
    }
}

file static class VotingExtensions
{
    public static UpdateVotingResult ToUpdateVotingResult(this Voting voting) =>
        new(
            voting.Id,
            voting.Title,
            voting.Description,
            voting.Type,
            voting.IsAnonymous,
            voting.AllowVoteChange,
            voting.StartTime,
            voting.EndTime,
            voting.Status,
            voting.CreatedAt,
            voting.CreatedById);
}
