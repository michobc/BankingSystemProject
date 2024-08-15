using System.IdentityModel.Tokens.Jwt;

namespace BankingSystemProject.Common.Services;

public class TokenExtractor
{
    // returns a dictionary containing the claims with there values
    public static IDictionary<string, string> ExtractClaimsFromToken(string token)
    {
        // Remove "Bearer " prefix if present
        if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            token = token.Substring("Bearer ".Length).Trim();
        }
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        return jwtToken.Claims.ToDictionary(c => c.Type, c => c.Value);
    }
}