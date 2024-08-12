using BankingSystemProject.Persistence.Data;

namespace BankingSystemProject.Persistence.Services;

using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

public class SchemaManagementService
{
    private readonly BankingSystemContext _dbContext;

    public SchemaManagementService(BankingSystemContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task CreateBranchSchemaAsync(string branchName)
    {
        var schemaSql = $"CREATE SCHEMA {branchName};";
        var usersTableSql = $@"
            CREATE TABLE {branchName}.users (
                user_id SERIAL PRIMARY KEY,
                username VARCHAR(100) UNIQUE NOT NULL,
                role VARCHAR(50) NOT NULL,
                branch_name VARCHAR(100),
                FOREIGN KEY (branch_name) REFERENCES public.branches(branch_name)
            );";
        var accountsTableSql = $@"
            CREATE TABLE {branchName}.accounts (
                account_id SERIAL PRIMARY KEY,
                user_id INT NOT NULL,
                balance DECIMAL(10, 2) NOT NULL,
                created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                FOREIGN KEY (user_id) REFERENCES {branchName}.users(user_id)
            );";
        var transactionsTableSql = $@"
            CREATE TABLE {branchName}.transactions (
                transaction_id SERIAL PRIMARY KEY,
                account_id INT NOT NULL,
                amount DECIMAL(10, 2) NOT NULL,
                transaction_type VARCHAR(50) NOT NULL,
                created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                FOREIGN KEY (account_id) REFERENCES {branchName}.accounts(account_id)
            );";

        using (var command = _dbContext.Database.GetDbConnection().CreateCommand())
        {
            command.CommandText = $"{schemaSql} {usersTableSql} {accountsTableSql} {transactionsTableSql}";
            command.CommandType = System.Data.CommandType.Text;

            await _dbContext.Database.OpenConnectionAsync();
            await command.ExecuteNonQueryAsync();
        }
    }
}