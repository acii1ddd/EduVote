using EduVote.API.Mappers;
using EduVote.API.Services.Tools;
using EduVote.DAL.Postgresql.Repositories.Interfaces;

namespace EduVote.API.Services.Grpc;

public class VotingTargetService(
    IVotingRepository votingRepository,
    IEducationUnitRepository educationUnitRepository,
    IVotingTargetRepository votingTargetRepository)
    : VotingTargets.VotingTargetsBase
{
    public override async Task<VotingTargetResponse> AddTarget(
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
            context.CancellationToken
        );
        
        if (educationUnit is null)
        {
            throw IdParser.CreateNotFoundException("EducationUnit", request.EducationUnitId);
        }
    
        var existingTarget = await votingTargetRepository.GetByVotingAndEducationUnitAsync(
            votingId,
            educationUnitId,
            context.CancellationToken
        );
    
        if (existingTarget is not null)
        {
            throw new RpcException(new Status(
                StatusCode.AlreadyExists,
                $"Target for voting '{votingId}' and education unit '{educationUnitId}' already exists."));
        }

        var createdTarget = await votingTargetRepository
            .CreateAsync(request.MapToEntity(), context.CancellationToken);

        return createdTarget.MapToResponse();
    }
    
    public override async Task<Empty> DeleteTarget(
        DeleteVotingTargetRequest request,
        ServerCallContext context)
    {
        var votingId = IdParser.ParseId(request.VotingId, "Voting");
        var educationUnitId = IdParser.ParseId(request.EducationUnitId, "EducationUnit");
    
        var voting = await votingRepository
            .GetByIdAsync(votingId, context.CancellationToken);
        
        if (voting is null)
        {
            throw IdParser.CreateNotFoundException("Voting", request.VotingId);
        }
        
        var target = await votingTargetRepository
            .GetByVotingAndEducationUnitAsync(votingId, educationUnitId, context.CancellationToken);
        
        // noting to delete
        if (target is null)
        {
            throw IdParser.CreateNotFoundException("VotingTarget", $"({votingId}, {educationUnitId})");
        }
        
        await votingTargetRepository
            .DeleteAsync(target, context.CancellationToken);
        
        return new Empty();
    }

    public override async Task<GetVotingTargetsResponse> GetTargets(
        GetVotingTargetsRequest request, 
        ServerCallContext context)
    {
        var votingId = IdParser.ParseId(request.VotingId, "Voting");
        
        var voting = await votingRepository
            .GetByIdAsync(votingId, context.CancellationToken);
        
        if (voting is null)
        {
            throw IdParser.CreateNotFoundException("Voting", request.VotingId);
        }
        
        var votingTargets = await votingTargetRepository.GetByVotingIdAsync(
            votingId, context.CancellationToken
        );

        var response = new GetVotingTargetsResponse();
        response.Targets.AddRange(votingTargets.MapToResponseList());

        return response;
    }
}
