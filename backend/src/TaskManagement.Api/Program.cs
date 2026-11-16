using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TaskManagement.Api.Configuration;
using TaskManagement.Api.Middleware;
using TaskManagement.Application;
using TaskManagement.Infrastructure;
using TaskManagement.Infrastructure.Identity;
using TaskManagement.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:4200"];
builder.Services.AddCors(options => options.AddPolicy("Frontend", policy =>
    policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddSwaggerConfig();

var jwtSection = builder.Configuration.GetSection(JwtOptions.SectionName);
builder.Services.Configure<JwtOptions>(jwtSection);
var jwtOptions = jwtSection.Get<JwtOptions>()
    ?? throw new InvalidOperationException("JWT settings are not configured.");
if (Encoding.UTF8.GetByteCount(jwtOptions.Key) < 32)
{
    throw new InvalidOperationException("Jwt:Key must be configured with at least 32 bytes of secret key material.");
}
if (string.IsNullOrWhiteSpace(jwtOptions.Issuer) ||
    string.IsNullOrWhiteSpace(jwtOptions.Audience) ||
    jwtOptions.ExpirationMinutes != 30)
{
    throw new InvalidOperationException("JWT issuer, audience, and expiration must be configured.");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            NameClaimType = System.Security.Claims.ClaimTypes.NameIdentifier
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHealthChecks()
    .AddDbContextCheck<TaskManagementDbContext>("database", tags: ["ready"]);

var app = builder.Build();

app.MapGet("/", async context =>
{
    context.Response.Redirect("/swagger");
    await Task.CompletedTask;
});

app.UseMiddleware<ApiExceptionMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            apiStatus = "Healthy",
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                component = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration.ToString()
            }),
            totalDuration = report.TotalDuration.ToString()
        };
        await context.Response.WriteAsJsonAsync(response);
    }
});
app.MapControllers();

await app.RunAsync();

public partial class Program;
