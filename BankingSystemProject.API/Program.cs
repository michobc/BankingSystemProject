using BankingSystemProject.API.Configurations;
using BankingSystemProject.API.Settings;
using BankingSystemProject.Application.Commands;
using BankingSystemProject.Application.Mappings;
using BankingSystemProject.Application.Services;
using BankingSystemProject.Application.Services.Abstractions;
using BankingSystemProject.Common.Services;
using BankingSystemProject.Infrastructure.Services;
using BankingSystemProject.Persistence.Data;
using BankingSystemProject.Persistence.Services;
using BankingSystemProject.Persistence.Services.Abstractions;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Register DbContext with Npgsql for PostgreSQL
builder.Services.AddDbContext<BankingSystemContext>();
builder.Services.AddScoped<DbContextFactory>();

// Register MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(typeof(Program).Assembly));
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(typeof(GetEmployee).Assembly));

// Register IMemoryCache
builder.Services.AddMemoryCache();

// Register AutoMapper
builder.Services.AddAutoMapper(typeof(BankingSystemMappings).Assembly);

// JWT configuration
var jwtSettings = new JwtSettings();
builder.Configuration.GetSection(nameof(JwtSettings)).Bind(jwtSettings);
builder.Services.AddSingleton(jwtSettings);

// Register API versioning
var apiVersioningSettings = builder.Configuration.GetSection("ApiVersioning");
var defaultApiVersion = new ApiVersion(apiVersioningSettings.GetValue<int>("MajorVersion"), apiVersioningSettings.GetValue<int>("MinorVersion"));
var assumeDefaultVersionWhenUnspecified = apiVersioningSettings.GetValue<bool>("AssumeDefaultVersionWhenUnspecified");

builder.Services.AddApiVersioning(options =>
{
    options.ReportApiVersions = true;
    options.AssumeDefaultVersionWhenUnspecified = assumeDefaultVersionWhenUnspecified;
    options.DefaultApiVersion = defaultApiVersion;
});

// Register Services
builder.Services.AddHttpClient();
builder.Services.AddScoped<IKeycloakAuthService, KeycloakAuthService>();
builder.Services.AddSingleton<ITenantService, TenantService>();
builder.Services.AddScoped<IGetAllAccountsService, GetAllAccountsService>();
builder.Services.AddScoped<TokenExtractor>();
builder.Services.AddScoped<CalculatNextTransactionDate>();
builder.Services.AddHttpContextAccessor();
// Register RabbitMQ service
builder.Services.AddSingleton<RabbitMqService>();

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.MongoDB(databaseUrl: builder.Configuration.GetConnectionString("MongoDBConnection"),
        collectionName: "logs")
    .CreateLogger();

builder.Host.UseSerilog();

// Register Health Checks
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection"))
    .AddMongoDb(mongodbConnectionString: builder.Configuration.GetConnectionString("MongoDBConnection"),
        name: "mongodb"
    );

// Register HealthChecks UI
builder.Services.AddHealthChecksUI()
    .AddInMemoryStorage();

// Swagger configuration
builder.Services.AddSwaggerServices();
builder.Services.AddAuthenticationServices(builder.Configuration);

builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



var app = builder.Build();

// Initialize tenant data
using (var scope = app.Services.CreateScope())
{
    var tenantService = scope.ServiceProvider.GetRequiredService<ITenantService>();
    var dbContext = scope.ServiceProvider.GetRequiredService<BankingSystemContext>();
    
    // Fetch tenant data and initialize the TenantService
    var branches = await dbContext.Branches.ToListAsync();
    tenantService.InitializeTenantSchemas(branches);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthenticationServices();

app.MapControllers();

// Map Health Check endpoints
app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecksUI(setup =>
{
    setup.UIPath = "/health-ui";
    setup.ApiPath = "/health-ui-api";
});

app.Run();