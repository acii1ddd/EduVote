using EduVote.DAL.Postgresql.Models;
using EduVote.DAL.Postgresql.Repositories;

namespace EduVote.API.Services;

public class VotingTargetService(
    IVotingRepository votingRepository,
    IEducationUnitRepository educationUnitRepository,
    IVotingTargetRepository votingTargetRepository)
    : VotingTargets.VotingTargetsBase
{
    public override async Task<VotingTargetResponse> AddTargetAsync(
        AddVotingTargetRequest request,
        ServerCallContext context)
    {
        var votingId = IdParser.ParseId(request.VotingId, "Voting");
        var educationUnitId = IdParser.ParseId(request.EducationUnitId, "EducationUnit");

        var voting = await votingRepository.GetByIdAsync(votingId, context.CancellationToken);
        if (voting is null)
        {
            throw IdParser.CreateNotFoundException("Voting", request.VotingId);
        }

        var educationUnit = await educationUnitRepository.GetByIdAsync(
            educationUnitId,
            context.CancellationToken);
        if (educationUnit is null)
        {
            throw IdParser.CreateNotFoundException("EducationUnit", request.EducationUnitId);
        }

        var existingTarget = await votingTargetRepository.GetByVotingAndEducationUnitAsync(
            votingId,
            educationUnitId,
            context.CancellationToken);

        if (existingTarget is not null)
        {
            throw new RpcException(new Status(
                StatusCode.AlreadyExists,
                $"Target for voting '{votingId}' and education unit '{educationUnitId}' already exists."));
        }

        var target = new VotingTarget
        {
            VotingId = votingId,
            EducationUnitId = educationUnitId
        };

        await votingTargetRepository.CreateAsync(target, context.CancellationToken);

        return new VotingTargetResponse
        {
            VotingId = votingId.ToString(),
            EducationUnitId = educationUnitId.ToString()
        };
    }

    public override async Task<Empty> DeleteTargetAsync(
        DeleteVotingTargetRequest request,
        ServerCallContext context)
    {
        var votingId = IdParser.ParseId(request.VotingId, "Voting");
        var targetId = IdParser.ParseId(request.TargetId, "VotingTarget");

        var voting = await votingRepository.GetByIdAsync(votingId, context.CancellationToken);
        if (voting is null)
        {
            throw IdParser.CreateNotFoundException("Voting", request.VotingId);
        }
        // todo переделать по составному ключу
        var target = await votingTargetRepository.GetByIdAsync(targetId, context.CancellationToken);
        if (target is null)
        {
            throw IdParser.CreateNotFoundException("VotingTarget", request.TargetId);
        }

        if (target.VotingId != votingId)
        {
            throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                $"Target '{targetId}' does not belong to voting '{votingId}'."));
        }

        await votingTargetRepository.DeleteAsync(target, context.CancellationToken);
        return new Empty();
    }
    
    // todo операции чтения 
}
