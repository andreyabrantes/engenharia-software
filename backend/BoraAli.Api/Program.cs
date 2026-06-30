using System.Data;
using System.Text;
using System.Threading.RateLimiting;
using BoraAli.Api.Extensions;
using BoraAli.Api.Services;
using BoraAli.Infrastructure.Data;
using BoraAli.Infrastructure.Middleware;
using BoraAli.Infrastructure.Repositories;
using BoraAli.Core.Interfaces;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;

using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ===== Serilog Configuration =====
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "Logs/boraali-.log",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

// ===== Database Configuration =====
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddSingleton(new DbSession(connectionString));

// ===== Database Migrations (DbUp) =====
// Executa scripts SQL versionados da pasta Migrations/ automaticamente.
// O DbUp controla quais scripts já foram executados via tabela SchemaVersions.
DatabaseInitializer.RunMigrations(connectionString);

// ===== Dependency Injection - Repositories =====
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IDapperExecutor, DapperExecutor>();

// ===== Dependency Injection - Services =====
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<EventService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<FavoriteService>();

// ===== AutoMapper =====
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

// ===== FluentValidation =====
builder.Services.AddFluentValidationAutoValidation()
    .AddFluentValidationClientsideAdapters();

builder.Services.AddTransient<CreateEventValidator>();
builder.Services.AddTransient<UpdateEventValidator>();
builder.Services.AddTransient<RegisterUserValidator>();
builder.Services.AddTransient<LoginValidator>();
builder.Services.AddTransient<CreateOrderValidator>();

// ===== JWT Authentication =====
var jwtSettings = builder.Configuration.GetSection("JwtSettings");

// Prioridade: Environment Variable > User Secrets > appsettings.json
var secretKey = builder.Configuration["JwtSettings:SecretKey"]
    ?? builder.Configuration.GetSection("JwtSettings")["SecretKey"]
    ?? throw new InvalidOperationException(
        "JWT SecretKey não configurada. Defina a variável de ambiente 'JwtSettings__SecretKey' " +
        "ou utilize 'dotnet user-secrets set \"JwtSettings:SecretKey\" \"<chave-aqui>\"'.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"] ?? "BoraAli",
        ValidAudience = jwtSettings["Audience"] ?? "BoraAliUsers",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ClienteOnly", policy =>
        policy.RequireRole("Cliente", "Admin"));

    options.AddPolicy("OrganizadorOnly", policy =>
        policy.RequireRole("Organizador", "Admin"));

    options.AddPolicy("ClienteOrOrganizador", policy =>
        policy.RequireRole("Cliente", "Organizador", "Admin"));
});

// ===== CORS Restrito =====
var allowedOrigins = builder.Configuration.GetSection("CorsSettings:AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:3000", "http://localhost:5188", "http://localhost:5000" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// ===== Rate Limiting =====
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Política global: 100 requisições por minuto por IP
    options.AddFixedWindowLimiter("Global", config =>
    {
        config.PermitLimit = 100;
        config.Window = TimeSpan.FromMinutes(1);
        config.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        config.QueueLimit = 0;
    });

    // Política restrita para login: 5 tentativas por minuto por IP
    options.AddFixedWindowLimiter("Login", config =>
    {
        config.PermitLimit = 5;
        config.Window = TimeSpan.FromMinutes(1);
        config.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        config.QueueLimit = 0;
    });

    // Política restrita para criação de pedidos: 10 por minuto por IP
    options.AddFixedWindowLimiter("Orders", config =>
    {
        config.PermitLimit = 10;
        config.Window = TimeSpan.FromMinutes(1);
        config.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        config.QueueLimit = 0;
    });

    // Política restrita para upload de imagem: 20 por minuto por IP
    options.AddFixedWindowLimiter("Upload", config =>
    {
        config.PermitLimit = 20;
        config.Window = TimeSpan.FromMinutes(1);
        config.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        config.QueueLimit = 0;
    });

    // Política restrita para check-in (QR Code): 5 por minuto por IP
    // Previne força bruta para adivinhar códigos de pedido
    options.AddFixedWindowLimiter("CheckIn", config =>
    {
        config.PermitLimit = 5;
        config.Window = TimeSpan.FromMinutes(1);
        config.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        config.QueueLimit = 0;
    });
});

// ===== Controllers =====
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// ===== Swagger/OpenAPI =====
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "BoraAli API",
        Version = "v1",
        Description = "API do BoraAli - Plataforma de Eventos e Ingressos",
        Contact = new OpenApiContact
        {
            Name = "BoraAli",
            Email = "contato@boraali.com.br"
        }
    });

    // Configuração do JWT no Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insira o token JWT no formato: Bearer {seu-token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// ===== Middleware Pipeline =====

// Exception Middleware (deve ser o primeiro)
app.UseMiddleware<ExceptionMiddleware>();

// Swagger (disponível em todos os ambientes)
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "BoraAli API v1");
    options.RoutePrefix = "swagger";
});

// Rate Limiting (antes do CORS e demais middlewares)
app.UseRateLimiter();

// CORS
app.UseCors("AllowFrontend");

// HSTS (força HTTPS em produção)
if (app.Environment.IsProduction())
{
    app.UseHsts();
}

// HTTPS Redirection
app.UseHttpsRedirection();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Static Files (serve wwwroot for uploaded images)
app.UseStaticFiles();

// Map Controllers
app.MapControllers();

// Health check endpoint
app.MapGet("/", () => Results.Redirect("/swagger"));
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow }));

try
{
    Log.Information("Iniciando BoraAli API");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "A aplicação falhou ao iniciar");
}
finally
{
    Log.CloseAndFlush();
}
