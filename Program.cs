using System.Threading.RateLimiting;
using backend.Classes;
using backend.Constants;
using backend.Services.Auth;
using backend.Services.LiveRecordings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// 1. Core Services
builder.Services.AddOpenApi();
builder.Services.AddOutputCache();
builder.Services.AddRateLimiter();
builder.Services.AddCors();

// 2. Application Services

// Register strongly-typed settings from configuration
builder.Services.Configure<AuthSettings>(configuration.GetSection("AuthSettings"));
builder.Services.Configure<ApiSettings>(configuration.GetSection("ApiSettings"));

// Register HTTP clients for the services

// IAuthService uses a HttpClient to contact the Identity Server.
builder.Services.AddHttpClient<IAuthService, AuthService>();

// ILiveRecordingsService uses a separate HttpClient to contact the ClarifyGo API.
var clarifyGoApiUri = configuration["ApiSettings:ApiBaseUri"] ??
                      throw new InvalidOperationException("ClarifyGo API URI not configured.");
builder.Services.AddHttpClient<ILiveRecordingsService, LiveRecordingsService>(client =>
{
    client.BaseAddress = new Uri(clarifyGoApiUri);
});

// Register Authentication & Authorization.
// NOTE: In production, update the token validation parameters with the proper issuer, audience, and signing key.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,       // change to true and set ValidIssuer for production
            ValidateAudience = false,     // change to true and set ValidAudience for production
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            // IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("Secret-Key"))
        };
    });
builder.Services.AddAuthorization();



// 3. Policies
ConfigurePolicies(builder.Services);

var app = builder.Build();

// 4. Pipeline Configuration
ConfigurePipeline(app);

// 5. Endpoints
ConfigureEndpoints(app);

app.Run();


// --- Local Functions ---

void ConfigurePolicies(IServiceCollection services)
{
    // Configure a sliding window rate limiter per user or IP address
    services.Configure<RateLimiterOptions>(options =>
    {
        options.AddPolicy<string>("PerUserPolicy", context =>
            RateLimitPartition.GetSlidingWindowLimiter(
                context.User.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString(),
                _ => new SlidingWindowRateLimiterOptions
                {
                    Window = TimeSpan.FromMinutes(1),
                    PermitLimit = 100
                }));
    });

    // Configure output caching with a policy for recordings
    services.Configure<OutputCacheOptions>(options =>
    {
        options.AddPolicy("RecordingsCache", builder =>
            builder.Expire(TimeSpan.FromMinutes(5))
                   .SetVaryByQuery("*")
                   .Tag("recordings"));
    });

    // Configure CORS with allowed origins from configuration
    services.AddCors(options =>
    {
        options.AddPolicy("RestrictedOrigins", policy =>
        {
            policy.WithOrigins(configuration.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>())
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    });
}

void ConfigurePipeline(WebApplication app)
{
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    app.UseHsts();
    app.UseHttpsRedirection();
    app.UseCors("RestrictedOrigins");
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseRateLimiter();
    app.UseOutputCache();
}

void ConfigureEndpoints(WebApplication app)
{
    // Health check endpoint
    app.MapGet("/health", () => Results.Ok())
       .AllowAnonymous();

    // Live Recordings endpoint (requires valid JWT and uses caching)
    app.MapGet(ClarifyGoApiEndpoints.LiveRecordings.GetAll,
        async (ILiveRecordingsService service) =>
            await service.GetLiveRecordingsAsync())
       .RequireAuthorization()
       .CacheOutput("RecordingsCache");
}

// --- Extension Method for OpenAPI ---
static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOpenApi(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        return services;
    }
}
