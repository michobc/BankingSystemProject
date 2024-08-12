using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using BankingSystemProject.Domain.Models;
using BankingSystemProject.Persistence.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BankingSystemProject.Persistence.Data;

public partial class BankingSystemContext : DbContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly string _schema;

    public BankingSystemContext(DbContextOptions<BankingSystemContext> options,  IHttpContextAccessor httpContextAccessor)
        : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
        _schema = GetSchemaForTenant();
    }
    
    private string GetSchemaForTenant()
    {
        var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].FirstOrDefault();
        var claims = ExtractClaimsFromToken(token);
        return TenantProvider.GetSchemaForTenant(claims["branchId"], this);
    }
    
    private static IDictionary<string, string> ExtractClaimsFromToken(string token)
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

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<Branch> Branches { get; set; }

    public virtual DbSet<Event> Events { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (_schema != null)
        {
            optionsBuilder.UseNpgsql($"Host=localhost;Database=bankingsystemdb;Username=postgres;Password=mypass03923367;Search Path={_schema}");
        }
        else
        {
            throw new InvalidOperationException("Schema not set. Ensure tenant context is properly configured.");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.AccountId).HasName("accounts_pkey");

            entity.ToTable("accounts");

            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.Balance)
                .HasPrecision(10, 2)
                .HasColumnName("balance");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Accounts)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("accounts_user_id_fkey");
        });

        modelBuilder.Entity<Branch>(entity =>
        {
            entity.HasKey(e => e.BranchName).HasName("branches_pkey");

            entity.ToTable("branches");

            entity.Property(e => e.BranchName)
                .HasMaxLength(100)
                .HasColumnName("branch_name");
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.EventId).HasName("events_pkey");

            entity.ToTable("events");

            entity.Property(e => e.EventId).HasColumnName("event_id");
            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.BranchName)
                .HasMaxLength(100)
                .HasColumnName("branch_name");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.EventData)
                .HasColumnType("jsonb")
                .HasColumnName("event_data");
            entity.Property(e => e.EventType)
                .HasMaxLength(50)
                .HasColumnName("event_type");
            entity.Property(e => e.TransactionId).HasColumnName("transaction_id");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.TransactionId).HasName("transactions_pkey");

            entity.ToTable("transactions");

            entity.Property(e => e.TransactionId).HasColumnName("transaction_id");
            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.Amount)
                .HasPrecision(10, 2)
                .HasColumnName("amount");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.TransactionType)
                .HasMaxLength(50)
                .HasColumnName("transaction_type");

            entity.HasOne(d => d.Account).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("transactions_account_id_fkey");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("users_pkey");

            entity.ToTable("users");

            entity.HasIndex(e => e.Username, "users_username_key").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.BranchName)
                .HasMaxLength(100)
                .HasColumnName("branch_name");
            entity.Property(e => e.Role)
                .HasMaxLength(50)
                .HasColumnName("role");
            entity.Property(e => e.Username)
                .HasMaxLength(100)
                .HasColumnName("username");

            entity.HasOne(d => d.BranchNameNavigation).WithMany(p => p.Users)
                .HasForeignKey(d => d.BranchName)
                .HasConstraintName("users_branch_name_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}


public static class TenantProvider
{
    private static Dictionary<string, string> _tenantSchemas = new()
    {
        
    };

    public static string GetSchemaForTenant(string tenantId, BankingSystemContext context)
    {
        if (_tenantSchemas.TryGetValue(tenantId, out var schema))
        {
            return schema;
        }
        // Default schema or handle unknown tenant
        // await CreateNewBranchAsync(tenantId, context);
        return "public";
    }
    
    public static async Task CreateNewBranchAsync(string branchName, BankingSystemContext dbContext)
    {
        // Create the branch in the branches table
        var createBranchSql = $"INSERT INTO public.branches (branch_name) VALUES ('{branchName}');";
        await dbContext.Database.ExecuteSqlRawAsync(createBranchSql);

        // Create schema and tables for the new branch
        var schemaManagementService = new SchemaManagementService(dbContext);
        await schemaManagementService.CreateBranchSchemaAsync(branchName);

        // Add the new branch to the tenant schemas
        _tenantSchemas[branchName] = branchName;
    }
}