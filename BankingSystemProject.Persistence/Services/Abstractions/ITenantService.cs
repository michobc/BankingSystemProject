using BankingSystemProject.Domain.Models;

namespace BankingSystemProject.Persistence.Services.Abstractions;

public interface ITenantService
{
    public string GetSchema();
    public void SetSchema(string schema);
    string GetSchemaForTenant(string tenantId);
    public void InitializeTenantSchemas(IEnumerable<Branch> branches);

    public void setUsername(string username);
    public void setRole(string role);
    public string getUsername();
    public string getRole();
}