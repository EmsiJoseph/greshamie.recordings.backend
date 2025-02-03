using System.Threading.RateLimiting;
using backend.Constants;
using backend.Services.Audit;
using backend.Services.ClarifyGo;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// 1. Core Services
builder.Services.AddOpenApi();
builder.Services.AddOutputCache();
builder.Services.AddRateLimiter();

// 2. Authentication & Authorization
ConfigureAuth(builder.Services, builder.Configuration);

// 3. ClarifyGo Configuration
ConfigureClarifyGo(builder.Services, builder.Configuration);

// 4. Application Services
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IClarifyGoService, ClarifyGoService>();

// 5. Policies
ConfigurePolicies(builder.Services);

var app = builder.Build();

// 6. Pipeline Configuration
ConfigurePipeline(app);

// 7. Endpoints
ConfigureEndpoints(app);

app.Run();

// Configuration Methods
void ConfigureAuth(IServiceCollection services, IConfiguration configuration)
{
    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.Authority = configuration["Auth:Authority"];
            options.Audience = configuration["Auth:Audience"];
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true
            };
        });

    services.AddAuthorization(options =>
    {
        options.AddPolicy("RequireAuditAccess", policy =>
            policy.RequireClaim("scope", "audit.write"));
    });
}

void ConfigureClarifyGo(IServiceCollection services, IConfiguration configuration)
{
    var clarifyGoConfig = configuration.GetSection("ClarifyGo");
    var baseUrl = clarifyGoConfig["BaseUrl"];
    
    services.AddHttpClient("ClarifyGoAuth")
        .AddClientCredentialsTokenManagement(options =>
        {
            options.Client.Clients.Add("clarifygo", new ClientCredentialsTokenRequest
            {
                Address = $"{baseUrl}/Identity/connect/token",
                ClientId = clarifyGoConfig["ClientId"],
                ClientSecret = clarifyGoConfig["ClientSecret"],
                Scope = "WebApi"
            });
        });

    services.AddHttpClient("ClarifyGoAPI", client => 
    {
        client.BaseAddress = new Uri($"{baseUrl}/API/");
    })
    .AddClientAccessTokenHandler("clarifygo");
}

void ConfigurePolicies(IServiceCollection services)
{
    services.Configure<RateLimiterOptions>(options =>
    {
        options.AddPolicy<string>("PerUserPolicy", context =>
            RateLimitPartition.GetSlidingWindowLimiter(
                context.User.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString(),
                _ => new SlidingWindowRateLimiterOptions
                {
                    Window = TimeSpan.FromMinutes(1),
                    PermissionsPerWindow = 100
                }));
    });

    services.Configure<OutputCacheOptions>(options =>
    {
        options.AddPolicy("RecordingsCache", builder =>
            builder.Expire(TimeSpan.FromMinutes(5))
                .SetVaryByQuery("*")
                .Tag("recordings"));
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
	// Pattern of mapping endpoints to services

	//app.MapGet(<Call here the endpoint from AppApiEndpoints>, async (<Interface> <Service>) =>
	//  await <Service>.<Method>Async())
    // .RequireAuthorization()
    // .RequireRateLimiting("PerUserPolicy")
    // .CacheOutput("RecordingsCache");

    app.MapGet(AppApiEndpoints.LiveRecordingsUri, async (IClarifyGoService clarifyGoService) =>
        await clarifyGoService.GetLiveRecordingsAsync())
        .RequireAuthorization()
        .RequireRateLimiting("PerUserPolicy")
        .CacheOutput("RecordingsCache");

}

