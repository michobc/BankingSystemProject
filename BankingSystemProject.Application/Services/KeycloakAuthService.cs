using BankingSystemProject.Application.Services.Abstractions;
using BankingSystemProject.Common.Services;
using BankingSystemProject.Domain.DTOs;
using BankingSystemProject.Persistence.Data;
using BankingSystemProject.Persistence.Services.Abstractions;
using Newtonsoft.Json.Linq;

namespace BankingSystemProject.Application.Services;


public class KeycloakAuthService : IKeycloakAuthService
{
    private readonly HttpClient _httpClient;
    private readonly string _tokenEndpoint;
    private readonly ITenantService _tenantService;
    private readonly BankingSystemContext _context;

    public KeycloakAuthService(HttpClient httpClient, ITenantService tenantService, BankingSystemContext context)
    {
        _httpClient = httpClient;
        _tokenEndpoint = "http://localhost:8080/realms/BakingSystemRealm/protocol/openid-connect/token";
        _tenantService = tenantService;
        _context = context;
    }

    public async Task<string> AuthenticateAsync(LoginDto request)
    {
        // Create the request content
        var requestContent = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("client_id", "BakingSystemClient"),
            new KeyValuePair<string, string>("client_secret", "mySecretKey"),
            new KeyValuePair<string, string>("grant_type", "password"),
            new KeyValuePair<string, string>("username", request.Username),
            new KeyValuePair<string, string>("password", request.Password)
        });

        try
        {
            var response = await _httpClient.PostAsync(_tokenEndpoint, requestContent);

            // Check if the response was successful
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error from Keycloak: {errorContent}");
            }

            var content = await response.Content.ReadAsStringAsync();

            var jsonResponse = JObject.Parse(content);

            var accessToken = jsonResponse["access_token"]?.ToString();

            if (string.IsNullOrEmpty(accessToken))
            {
                throw new Exception("Authentication failed: Access token not found.");
            }
            
            var claims = TokenExtractor.ExtractClaimsFromToken(accessToken);
            // You can log or use the claims as needed
            Console.WriteLine($"User ID: {claims["sub"]}");
            Console.WriteLine($"Username: {claims["preferred_username"]}");
            Console.WriteLine($"Role: {claims["role"]}");
            Console.WriteLine($"Branch: {claims["branchId"]}");

            var tenant = _tenantService.GetSchemaForTenant(claims["branchId"]);
            if (tenant == null)
            {
                throw new Exception("branch do not exist");
            }
            else
            {
                _tenantService.SetSchema(claims["branchId"]);
                _tenantService.setUsername(claims["preferred_username"]);
                _tenantService.setRole(claims["role"]);
                return accessToken;
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"An error occurred while authenticating: {ex.Message}", ex);
        }
    }
}