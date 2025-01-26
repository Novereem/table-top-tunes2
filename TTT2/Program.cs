using Shared.Interfaces.Data;
using Shared.Interfaces.Services;
using Shared.Interfaces.Services.Common;
using Shared.Interfaces.Services.Helpers;
using TTT2.Data;
using TTT2.Services;
using TTT2.Services.Common;
using TTT2.Services.Helpers;

var builder = WebApplication.CreateBuilder(args);

//Load Environment Variables
DotNetEnv.Env.Load();

//Services Registration
builder.Services.AddScoped<IPasswordHashingService, PasswordHashingService>();

builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IAuthenticationData, AuthenticationData>();

builder.Services.AddScoped<IAuthenticationServiceHelper, AuthenticationServiceHelper>();


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
