using BankingSystemProject.Domain.Models;
using BankingSystemProject.Persistence.Services.Abstractions;

namespace BankingSystemProject.Persistence.Services;

public class TenantService : ITenantService
{
    private Dictionary<string, string> _tenantSchemas;
    private string _username;
    private string _role;
    private string _schema;

    public void SetSchema(string schema)
    {
        _schema = schema;
    }

    public string GetSchema()
    {
        return _schema;
    }

    public TenantService()
    {
        _tenantSchemas = new Dictionary<string, string>();
    }

    public void InitializeTenantSchemas(IEnumerable<Branch> branches)
    {
        _tenantSchemas.Clear();
        foreach (var branch in branches)
        {
            _tenantSchemas[branch.BranchName] = branch.BranchName;
        }
        Console.WriteLine(_tenantSchemas.Values);
    }

    public string GetSchemaForTenant(string tenantId)
    {
        return _tenantSchemas.TryGetValue(tenantId, out var schema) ? schema : null;
    }

    public void setRole(string role)
    {
        _role = role;
    }
    public string getRole()
    {
        return _role;
    }
    public void setUsername(string username)
    {
        _username = username;
    }
    public string getUsername()
    {
        return _username;
    }
}