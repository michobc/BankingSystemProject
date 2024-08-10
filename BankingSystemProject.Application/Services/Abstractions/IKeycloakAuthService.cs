using BankingSystemProject.Domain.DTOs;

namespace BankingSystemProject.Application.Services.Abstractions;

public interface IKeycloakAuthService
{
    public Task<string> AuthenticateAsync(LoginDto request);
}