using FGC.Users.Application.UseCases;
using FGC.Users.Domain.Interfaces;
using FGC.Users.Infrastructure.Data.Context;
using FGC.Users.Infrastructure.Repositories;
using FGC.Users.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

#region [Services - Controllers & Swagger]

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "FGC Users API",
        Version = "v1",
        Description = "Microsserviço de Usuários - FIAP Cloud Games"
    });

    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Insira o token JWT no formato: Bearer {seu_token}"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

#endregion

#region [Services - Authentication & Authorization]

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtSettings = builder.Configuration.GetSection("Jwt");
    var secretKey = jwtSettings["SecretKey"];

    if (string.IsNullOrEmpty(secretKey))
        throw new InvalidOperationException("JWT SecretKey não configurada");

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminPolicy", policy => policy.RequireRole("Admin"));
});

#endregion

#region [Services - Database & Repositories]

builder.Services.AddDbContext<UsersDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null));
});

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserUniquenessService, UserUniquenessService>();
builder.Services.AddScoped<IJwtService, JwtService>();

#endregion

#region [Services - Use Cases]

builder.Services.AddScoped<RegisterUserUseCase>();
builder.Services.AddScoped<AuthenticateUserUseCase>();
builder.Services.AddScoped<GetUserProfileUseCase>();
builder.Services.AddScoped<ChangePasswordUseCase>();
builder.Services.AddScoped<DeactivateUserUseCase>();
builder.Services.AddScoped<ReactivateUserUseCase>();
builder.Services.AddScoped<CreateAdminUserUseCase>();
builder.Services.AddScoped<PromoteUserToAdminUseCase>();
builder.Services.AddScoped<DemoteAdminToUserUseCase>();

#endregion

#region [Services - CORS & HealthChecks]

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddHealthChecks().AddDbContextCheck<UsersDbContext>();

#endregion

var app = builder.Build();

#region [Middleware - Swagger]

if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "FGC Users API v1");
        c.RoutePrefix = string.Empty;
    });
}

#endregion

#region [Middleware - Pipeline]

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.MapGet("/info", () => new
{
    Service = "FGC Users API",
    Version = "1.0.0",
    Status = "Running",
    Timestamp = DateTime.UtcNow
});

#endregion

#region [Database Initialization]

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
    try
    {
        Console.WriteLine("?? Aplicando migrations...");
        await context.Database.MigrateAsync();
        Console.WriteLine("? Banco de dados atualizado");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"? Erro ao aplicar migrations: {ex.Message}");
    }
}

#endregion

Console.WriteLine("???????????????????????????????????????????");
Console.WriteLine("?? FGC Users API - Microsserviço de Usuários");
Console.WriteLine("???????????????????????????????????????????");
Console.WriteLine($"?? Ambiente: {app.Environment.EnvironmentName}");
Console.WriteLine("?? Swagger: Habilitado");
Console.WriteLine("???????????????????????????????????????????");

app.Run();