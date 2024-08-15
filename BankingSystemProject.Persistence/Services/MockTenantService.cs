using BankingSystemProject.Domain.Models;
using BankingSystemProject.Persistence.Services.Abstractions;

namespace BankingSystemProject.Persistence.Services;

// Mock or implement this class to provide necessary tenant information
public class MockTenantService : ITenantService
{
    private string schema;
    public string GetSchema() => "public"; // Default schema for design-time
    public void SetSchema(string schema) => schema = this.schema;

    public string GetSchemaForTenant(string tenantId)
    {
        return " ";
    }
    public void InitializeTenantSchemas(IEnumerable<Branch> branches){}
    public void setUsername(string username){}
    public void setRole(string role){}

    public string getUsername()
    {
        return "";
        
    }
    public string getRole()
    {
        return "";
    }
}