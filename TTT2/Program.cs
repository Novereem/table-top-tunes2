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
using TTT2.Data;
using TTT2.Services;
using TTT2.Services.Common.Authentication;
using TTT2.Services.Helpers;

var builder = WebApplication.CreateBuilder(args);

//Load Environment Variables
DotNetEnv.Env.Load();

//Services Registration
builder.Services.AddScoped<IPasswordHashingService, PasswordHashingService>();
builder.Services.AddSingleton<IUserClaimsService, UserClaimsService>();

builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IAuthenticationData, AuthenticationData>();
builder.Services.AddScoped<ISceneService, SceneService>();
builder.Services.AddScoped<ISceneData, SceneData>();

builder.Services.AddScoped<IAuthenticationServiceHelper, AuthenticationServiceHelper>();
builder.Services.AddScoped<ISceneServiceHelper, SceneServiceHelper>();

// CORS Policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("https://localhost:7040")
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
        OnAuthenticationFailed = context =>
        {
            if (context.Exception is SecurityTokenExpiredException)
            {
                // Create the invalid session response
                var response = new ApiResponse<object>(null, MessageRepository.GetMessage(MessageKey.Error_Unauthorized).UserMessage);
                    
                var jsonResponse = JsonSerializer.Serialize(response);

                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";

                return context.Response.WriteAsync(jsonResponse);
            }

            return Task.CompletedTask;
        }
    };
});

// Add services to the container.
builder.Services.AddControllers();
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
