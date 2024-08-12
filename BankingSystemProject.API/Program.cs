using BankingSystemProject.API.Configurations;
using BankingSystemProject.API.Settings;
using BankingSystemProject.Application.Commands;
using BankingSystemProject.Application.Mappings;
using BankingSystemProject.Application.Services;
using BankingSystemProject.Application.Services.Abstractions;
using BankingSystemProject.Persistence.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder(args);

// Register DbContext with Npgsql for PostgreSQL
builder.Services.AddDbContext<BankingSystemContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddHttpContextAccessor();

// Register MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(typeof(Program).Assembly));
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(typeof(GetEmployee).Assembly));

// Register IMemoryCache
builder.Services.AddMemoryCache();

// Register AutoMapper
builder.Services.AddAutoMapper(typeof(BankingSystemMappings).Assembly);

// Swagger configuration
builder.Services.AddSwaggerServices();
builder.Services.AddAuthenticationServices(builder.Configuration);

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

builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();