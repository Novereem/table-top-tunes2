using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Shared.Enums;
using Shared.Interfaces.Data;
using Shared.Interfaces.Services;
using Shared.Interfaces.Services.Common.Authentication;
using Shared.Interfaces.Services.Helpers;
using Shared.Models.Common;
using Shared.Statics;
using System.Text;
using System.Text.Json;
using Shared.Interfaces.Controllers;
using Shared.Interfaces.Services.Helpers.FileValidation;
using Shared.Interfaces.Services.Helpers.Shared;
using Shared.Services.Converters;
using TTT2.Data;
using TTT2.Services;
using TTT2.Services.Common.Authentication;
using TTT2.Services.Helpers;
using TTT2.Services.Helpers.FileValidation;
using TTT2.Services.Helpers.Shared;

var builder = WebApplication.CreateBuilder(args);

//Load Environment Variables
DotNetEnv.Env.Load();

//Services Registration
//Common
builder.Services.AddScoped<IPasswordHashingService, PasswordHashingService>();
builder.Services.AddSingleton<IUserClaimsService, UserClaimsService>();
builder.Services.AddHealthChecks();
    
//Standard Services
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<ISceneService, SceneService>();
builder.Services.AddScoped<IAudioService, AudioService>();
builder.Services.AddScoped<ISceneAudioService, SceneAudioService>();
builder.Services.AddScoped<IUserStorageService, UserStorageService>();
builder.Services.AddScoped<IAudioStreamingService, AudioStreamingService>();

//Standard Data
builder.Services.AddScoped<IAuthenticationData, AuthenticationData>();
builder.Services.AddScoped<ISceneData, SceneData>();
builder.Services.AddScoped<IAudioData, AudioData>();
builder.Services.AddScoped<ISceneAudioData, SceneAudioData>();

//Helpers
builder.Services.AddScoped<IAuthenticationServiceHelper, AuthenticationServiceHelper>();
builder.Services.AddScoped<ISceneServiceHelper, SceneServiceHelper>();
builder.Services.AddScoped<IAudioServiceHelper, AudioServiceHelper>();
builder.Services.AddScoped<ISceneAudioServiceHelper, SceneAudioServiceHelper>();
builder.Services.AddScoped<IAudioStreamingServiceHelper, AudioStreamingServiceHelper>();

//Shared helpers
builder.Services.AddScoped<ISceneValidationService, SceneValidationService>();

//Shared
var isDevelopment = bool.TryParse(Environment.GetEnvironmentVariable("DEVELOPMENT"), out var devMode) && devMode;
if (isDevelopment)
{
    builder.Services.AddScoped<IHttpResponseConverter, DebugHttpResponseConverter>();
}
else
{
    builder.Services.AddScoped<IHttpResponseConverter, ProductionHttpResponseConverter>();
}

//Validators
builder.Services.AddScoped<IAudioFileValidator, AudioFileValidator>();
builder.Services.AddScoped<IFileSafetyValidator, FileSafetyValidator>();

// CORS Policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

//Set up authentication
var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "https://localhost:7062";
var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "https://localhost:7040";
var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");
if (string.IsNullOrEmpty(secretKey))
{
    throw new InvalidOperationException("JWT secret key is not set in the .env file.");
}

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
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };

    options.Events = new JwtBearerEvents
    {
        OnChallenge = async ctx =>
        {
            // suppress the default 401 handler
            ctx.HandleResponse();

            ctx.Response.StatusCode  = StatusCodes.Status401Unauthorized;
            ctx.Response.ContentType = "application/json";

            var payload = new ApiResponse<object>(
                null,
                MessageRepository.GetMessage(MessageKey.Error_Unauthorized).UserMessage);

            await ctx.Response.WriteAsync(JsonSerializer.Serialize(payload));
        }
    };
});

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// BIND TO ALL NETWORK INTERFACES ON PORT 80
builder.WebHost.UseUrls("http://0.0.0.0:80");

var app = builder.Build();

app.MapHealthChecks("/health");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

app.UseCors("AllowFrontend");

if (!app.Environment.IsProduction())
{
    app.UseHttpsRedirection(); // only in local dev
    Console.WriteLine("UseHttpsRedirection is ENABLED");
    Console.WriteLine("UseHttpsRedirection is ENABLED");
    Console.WriteLine("UseHttpsRedirection is ENABLED");
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }