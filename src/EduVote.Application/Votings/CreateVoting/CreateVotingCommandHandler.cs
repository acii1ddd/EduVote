using EduVote.DAL.Postgresql.Models;
using EduVote.DAL.Postgresql.Models.Roles;
using EduVote.DAL.Postgresql.Repositories.Interfaces;
using MediatR;
using DbVotingStatus = EduVote.DAL.Postgresql.Models.Enums.VotingStatus;

namespace EduVote.Application.Votings.CreateVoting;

public sealed class CreateVotingCommandHandler(IVotingRepository votingRepository)
    : IRequestHandler<CreateVotingCommand, CreateVotingResult>
{
    public async Task<CreateVotingResult> Handle(
        CreateVotingCommand request,
        CancellationToken cancellationToken)
    {
        CreateVotingValidator.ValidateDateRange(request.StartTime, request.EndTime);

        var voting = new Voting
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            Type = request.Type,
            IsAnonymous = request.IsAnonymous,
            AllowVoteChange = request.AllowVoteChange,
            StartTime = request.StartTime.ToUniversalTime(),
            EndTime = request.EndTime.ToUniversalTime(),
            Status = request.CreatorRole == Roles.Teacher
                ? DbVotingStatus.PendingApproval
                : DbVotingStatus.Draft
        };

        if (request.CreatedById.HasValue)
            voting.CreatedById = request.CreatedById.Value;

        var createdVoting = await votingRepository.CreateAsync(voting, cancellationToken);

        return createdVoting.ToCreateVotingResult();
    }
}

file static class VotingExtensions
{
    public static CreateVotingResult ToCreateVotingResult(this Voting voting) =>
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
