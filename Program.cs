using System.Text;
using backend.Data;
using backend.Data.Models;
using backend.Middleware;
using backend.Services.Audits;
using backend.Services.Auth;
using backend.Services.ClarifyGoServices.Comments;
using backend.Services.ClarifyGoServices.HistoricRecordings;
using backend.Services.ClarifyGoServices.LiveRecordings;
using backend.Services.ClarifyGoServices.Tags;
using backend.Services.Storage;
using backend.Services.Sync;
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

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            builder.Configuration["Jwt:Key"]!)),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
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

builder.Services.AddIdentityCore<User>(options => { })
    .AddRoles<Role>()
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

// 2.6 Audit Service
builder.Services.AddScoped<IAuditService, AuditService>();


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

builder.Services.AddHttpClient<ICommentsService, CommentsService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUri);
});

builder.Services.AddHttpClient<ISyncService, SyncService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUri);
});

builder.Services.AddHttpClient<ITagsService, TagsService>(client => { client.BaseAddress = new Uri(apiBaseUri); });

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
// app.UseHttpsRedirection();
app.UseCors("ReactClient");
app.UseAuthentication();
app.UseAuthorization();
app.UseOutputCache();
app.UseRateLimiter();
app.UseMiddleware<GlobalExceptionHandler>();
app.MapControllers();
app.Run();