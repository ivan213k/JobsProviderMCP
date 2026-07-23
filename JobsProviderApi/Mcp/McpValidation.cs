using System.ComponentModel.DataAnnotations;
using ModelContextProtocol;

namespace JobsProviderApi.Mcp;

internal static class McpValidation
{
    public static void ValidateOrThrow(object query)
    {
        var results = new List<ValidationResult>();
        if (!Validator.TryValidateObject(query, new ValidationContext(query), results, validateAllProperties: true))
        {
            throw new McpException(string.Join(" ", results.Select(r => r.ErrorMessage)));
        }
    }
}
