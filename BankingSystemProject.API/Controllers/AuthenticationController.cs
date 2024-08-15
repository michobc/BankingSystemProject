using BankingSystemProject.Application.Services.Abstractions;
using BankingSystemProject.Domain.DTOs;
using BankingSystemProject.Persistence.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace BankingSystemProject.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/auth")]
public class AuthenticationController(IKeycloakAuthService keycloakAuthService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> login([FromForm] LoginDto request)
    {
        var auth = await keycloakAuthService.AuthenticateAsync(request);
        return Ok(auth);
    }
}