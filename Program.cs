using backend.Classes;
using backend.Constants;
using backend.Services.Auth;
using backend.Services.ClarifyGoServices.LiveRecordings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// 1. Core Services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOutputCache();
builder.Services.AddControllers();
builder.Services.AddRateLimiter(options =>
{
    options.AddSlidingWindowLimiter("PerUserPolicy", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 100;
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = configuration["ClarifyGoAPI:IdentityServerUri"];
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = true,
            ValidIssuer = configuration["ClarifyGoAPI:IdentityServerUri"],
            ValidateLifetime = true
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                {
                    context.Response.Headers.Append("Token-Expired", "true");
                }

                return Task.CompletedTask;
            }
        };
    });

// 2. Application Services
// 2.1. Live Recordings Service
builder.Services.AddScoped<ILiveRecordingsService, LiveRecordingsService>();

// 2.1. Comment Service
builder.Services.AddScoped<ICommentsService, CommentsService>();

// 2.1. Token Service
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.Configure<ApiSettings>(configuration.GetSection("ApiSettings"));

// 3. HTTP Client Configurations
var identityServerUri = configuration["ClarifyGoAPI:IdentityServerUri"]
                        ?? throw new InvalidOperationException("Missing Identity Server URI configuration");

var apiBaseUri = configuration["ClarifyGoAPI:ApiBaseUri"]
                 ?? throw new InvalidOperationException("Missing API base URI configuration");

builder.Services.AddHttpClient<ITokenService, TokenService>(client =>
{
    client.BaseAddress = new Uri(identityServerUri);
});

builder.Services.AddHttpClient<ILiveRecordingsService, LiveRecordingsService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUri);
});

// 4. Security Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactClient", policy =>
    {
        policy.WithOrigins("https://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

// 5. Middleware Pipeline
// Add the Https Redirection Middleware in production
// app.UseHttpsRedirection();
app.UseCors("ReactClient");
app.UseOutputCache();
app.UseRateLimiter();

// 6. Endpoints
app.MapGet("/health", () => Results.Ok());

app.MapGet("/protected", (ITokenService tokenService) =>
    {
        var token = tokenService.GetAccessTokenFromContext();
        return Results.Ok(token);
    })
    .RequireAuthorization();

// 6.1. Live Recordings Endpoints
// 6.1.1. Get All Live Recordings
app.MapGet(AppApiEndpoints.LiveRecordings.GetAll,
        async (ILiveRecordingsService service) => await service.GetLiveRecordingsAsync())
    .CacheOutput("RecordingsCache");

// 6.1.2. Resume Recording
app.MapPost(AppApiEndpoints.LiveRecordings.PutResume,
        async (ILiveRecordingsService service, string recorderId, string recordingId) =>
        {
            await service.ResumeRecordingAsync(recorderId, recordingId);
            return Results.Ok();
        })
    .RequireRateLimiting("PerUserPolicy");

// 6.1.3. Pause Recording
app.MapPost(AppApiEndpoints.LiveRecordings.PutPause,
        async (ILiveRecordingsService service, string recorderId, string recordingId) =>
        {
            await service.PauseRecordingAsync(recorderId, recordingId);
            return Results.Ok();
        })
    .RequireRateLimiting("PerUserPolicy");


app.MapControllers();
app.Run();