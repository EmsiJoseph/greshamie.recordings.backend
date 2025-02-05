using System.Text;
using backend.Classes;
using backend.Constants;
using backend.Data;
using backend.DTOs;
using backend.Extensions;
using backend.Models;
using backend.Services.Auth;
using backend.Services.ClarifyGoServices.Comments;
using backend.Services.ClarifyGoServices.HistoricRecordings;
using backend.Services.ClarifyGoServices.LiveRecordings;
using backend.Services.ClarifyGoServices.Tags;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
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

// builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//     .AddJwtBearer(options =>
//     {
//         options.Authority = configuration["ClarifyGoAPI:IdentityServerUri"];
//         options.TokenValidationParameters = new TokenValidationParameters
//         {
//             ValidateAudience = false,
//             ValidateIssuer = true,
//             ValidIssuer = configuration["ClarifyGoAPI:IdentityServerUri"],
//             ValidateLifetime = true
//         };
//
//         options.Events = new JwtBearerEvents
//         {
//             OnAuthenticationFailed = context =>
//             {
//                 if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
//                 {
//                     context.Response.Headers.Append("Token-Expired", "true");
//                 }
//
//                 return Task.CompletedTask;
//             }
//         };
//     });

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                builder.Configuration["Jwt:Key"]!)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.WriteIndented = true;
    });

// Add db connection
var connection = String.Empty;

if (builder.Environment.IsDevelopment())

{
    connection = builder.Configuration.GetConnectionString("DefaultConnection") ??
                 throw new InvalidOperationException(
                     "Connection string 'DefaultConnection' not found.");
}
else
{
    connection = configuration["ConnectionStrings:DefaultConnection"];
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connection));

builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();


// 2. Application Services
// 2.1. Live Recordings Service
builder.Services.AddScoped<ILiveRecordingsService, LiveRecordingsService>();

// 2.2 Historic Recordings Service
builder.Services.AddScoped<IHistoricRecordingsService, HistoricRecordingsService>();

// 2.3. Comments Service
builder.Services.AddScoped<ICommentsService, CommentsService>();

// 2.4 Tags Service
builder.Services.AddScoped<ITagsService, TagsService>();

// 2.5. Token Service
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

builder.Services.AddHttpClient<IHistoricRecordingsService, HistoricRecordingsService>(client =>
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
app.MapGet(AppApiEndpoints.Recordings.Live.GetAll,
        async (ILiveRecordingsService service) => { return await service.GetLiveRecordingsAsync(); })
    .CacheOutput("RecordingsCache");

// 6.1.2. Resume Recording
app.MapPut(AppApiEndpoints.Recordings.Live.Resume,
        async (ILiveRecordingsService service, string recorderId, string recordingId) =>
        {
            await service.ResumeRecordingAsync(recorderId, recordingId);
            return Results.Ok();
        })
    .RequireRateLimiting("PerUserPolicy");

// 6.1.3. Pause Recording
app.MapPut(AppApiEndpoints.Recordings.Live.Pause,
        async (ILiveRecordingsService service, string recorderId, string recordingId) =>
        {
            await service.PauseRecordingAsync(recorderId, recordingId);
            return Results.Ok();
        })
    .RequireRateLimiting("PerUserPolicy");

// 6.1.4. Get Comments
app.MapGet(AppApiEndpoints.Recordings.Live.Comments,
        async (ICommentsService service, string recordingId) =>
        {
            bool isLiveRecording = true;
            await service.GetCommentsAsync(recordingId, isLiveRecording);
        })
    .CacheOutput("CommentsCache");

// 6.1.5. Post Comment
app.MapPost(AppApiEndpoints.Recordings.Live.Comments,
        async (ICommentsService service, string recordingId, string comment) =>
        {
            bool isLiveRecording = true;
            await service.PostCommentAsync(recordingId, comment, isLiveRecording);
        })
    .RequireRateLimiting("PerUserPolicy");

// 6.1.6. Delete Comment
app.MapDelete(AppApiEndpoints.Recordings.Live.CommentById,
        async (ICommentsService service, string recordingId, string commentId) =>
        {
            bool isLiveRecording = true;
            await service.DeleteCommentAsync(recordingId, commentId, isLiveRecording);
        })
    .RequireRateLimiting("PerUserPolicy");

// 6.1.7. Get Tags
app.MapGet(AppApiEndpoints.Recordings.Live.Tags,
        async (ITagsService service, string recordingId) =>
        {
            bool isLiveRecording = true;
            await service.GetTagsAsync(recordingId, isLiveRecording);
        })
    .CacheOutput("TagsCache");

// 6.1.8. Post Tag
app.MapPost(AppApiEndpoints.Recordings.Live.TagOperations,
        async (ITagsService service, string recordingId, string tag) =>
        {
            bool isLiveRecording = true;
            await service.PostTagAsync(recordingId, tag, isLiveRecording);
        })
    .RequireRateLimiting("PerUserPolicy");

// 6.1.9. Delete Tag
app.MapDelete(AppApiEndpoints.Recordings.Live.TagOperations,
        async (ITagsService service, string recordingId, string tag) =>
        {
            bool isLiveRecording = true;
            await service.DeleteTagAsync(recordingId, tag, isLiveRecording);
        })
    .RequireRateLimiting("PerUserPolicy");

// 6.2. Historic Recordings Endpoints
// 6.2.1. Get All Historic Recordings with default start and end dates
app.MapGet(AppApiEndpoints.Recordings.Historic.GetAll,
        async (IHistoricRecordingsService service,
            [AsParameters] RecordingSearchFiltersDto searchFiltersDto) =>
        {
            searchFiltersDto.StartDate = DateTime.Now.StartOfWeek(DayOfWeek.Monday);
            searchFiltersDto.EndDate = DateTime.Now;
            Console.WriteLine($"Start Date: {searchFiltersDto.StartDate}, End Date: {searchFiltersDto.EndDate}");
            return await service.SearchRecordingsAsync(searchFiltersDto);
        }
    )
    .CacheOutput("RecordingsCache");

// 6.2.2. Get All Historic Recordings with custom start and end dates
app.MapGet(AppApiEndpoints.Recordings.Historic.Search,
        async (IHistoricRecordingsService service,
                [AsParameters] RecordingSearchFiltersDto searchFiltersDto) =>
            await service.SearchRecordingsAsync(searchFiltersDto))
    .CacheOutput("RecordingsCache");

// 6.2.3. Delete Historic Recording
app.MapDelete(AppApiEndpoints.Recordings.Historic.Delete,
        async (IHistoricRecordingsService service, string recordingId) =>
        {
            await service.DeleteRecordingAsync(recordingId);
        })
    .RequireRateLimiting("PerUserPolicy");


app.MapControllers();
app.Run();