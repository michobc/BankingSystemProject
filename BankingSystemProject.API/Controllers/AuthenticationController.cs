using BankingSystemProject.Application.Services.Abstractions;
using BankingSystemProject.Domain.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace BankingSystemProject.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/auth")]
public class AuthenticationController(IKeycloakAuthService keycloakAuthService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> login([FromBody] LoginDto request)
    {
        var auth = await keycloakAuthService.AuthenticateAsync(request);
        return Ok(auth);
    }
}