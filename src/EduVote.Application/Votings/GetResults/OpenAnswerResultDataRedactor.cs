using System.Text.Json.Nodes;

namespace EduVote.Application.Votings.GetResults;

public static class OpenAnswerResultDataRedactor
{
    public static string RedactAnswerTexts(string resultData)
    {
        var root = JsonNode.Parse(resultData)?.AsObject();
        if (root is null || !root.TryGetPropertyValue("openAnswer", out var openNode))
            return resultData;

        if (openNode is not JsonObject openAnswer)
            return resultData;

        openAnswer["answers"] = new JsonArray();
        root["openAnswer"] = openAnswer;

        return root.ToJsonString();
    }
}
