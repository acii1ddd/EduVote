using EduVote.Application.Votings.SeedDemoVotes;
using MediatR;

namespace EduVote.API;

internal static class SeedDemoVotesRunner
{
    public static async Task<int> TryRunAsync(WebApplicationBuilder builder, string[] args)
    {
        if (args.Length < 2 || !string.Equals(args[0], "--seed-demo-votes", StringComparison.OrdinalIgnoreCase))
        {
            return -1;
        }

        if (!builder.Environment.IsDevelopment())
        {
            Console.Error.WriteLine("Demo vote seeding is allowed only in Development environment.");
            return 1;
        }

        var titleSubstring = args[1];
        var voteCount = 100;

        if (args.Length >= 3 && !int.TryParse(args[2], out voteCount))
        {
            Console.Error.WriteLine("Vote count must be a positive integer.");
            return 1;
        }

        if (voteCount < 1)
        {
            Console.Error.WriteLine("Vote count must be at least 1.");
            return 1;
        }

        var host = builder.Build();

        await using (host)
        {
        using var scope = host.Services.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var result = await sender.Send(new SeedDemoVotesCommand(titleSubstring, voteCount));

        Console.WriteLine(
            $"Seeded {result.VoteCount} demo votes for '{result.VotingTitle}' ({result.VotingId}). " +
            $"Created {result.DemoUsersCreated} demo users. " +
            "Finish the voting from the UI to calculate results.");
        }

        return 0;
    }
}