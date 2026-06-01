namespace EduVote.API.Validators;

/// <summary>
/// Utility class for parsing and validating GUIDs from string IDs.
/// Provides common methods for ID parsing across all services.
/// </summary>
public static class IdParser
{
    /// <summary>
    /// Parses a string ID to GUID.
    /// </summary>
    /// <param name="id">The string ID to parse</param>
    /// <param name="entityName">The name of the entity (used in error messages)</param>
    /// <returns>Parsed GUID</returns>
    /// <exception cref="RpcException">Thrown if the ID is not a valid GUID</exception>
    public static Guid ParseId(string id, string entityName = "Resource")
    {
        if (!Guid.TryParse(id, out var parsedId))
        {
            throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                $"{entityName} id must be a valid GUID."));
        }

        return parsedId;
    }

    /// <summary>
    /// Creates a NotFound RPC exception for the specified entity.
    /// </summary>
    /// <param name="entityName">The name of the entity</param>
    /// <param name="id">The ID that was not found</param>
    /// <returns>RpcException with NotFound status</returns>
    public static RpcException CreateNotFoundException(string entityName, string id)
    {
        return new RpcException(new Status(
            StatusCode.NotFound,
            $"{entityName} with id '{id}' was not found."));
    }
}
