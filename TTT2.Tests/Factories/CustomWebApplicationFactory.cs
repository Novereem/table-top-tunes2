using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Shared.Interfaces.Controllers;
using Shared.Interfaces.Data;
using Shared.Interfaces.Services;
using Shared.Services.Converters;
using TTT2.Tests.Endpoint;
using TTT2.Tests.FakeData;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
namespace TTT2.Tests.Factories
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override IHost CreateHost(IHostBuilder builder)
        {
            // Set the environment variables your application expects.
            Environment.SetEnvironmentVariable("JWT_SECRET_KEY", "test-secret-key-that-is-very-long-because-it-needs-to-be");
            Environment.SetEnvironmentVariable("JWT_ISSUER", "test-issuer");
            Environment.SetEnvironmentVariable("JWT_AUDIENCE", "test-audience");
            Environment.SetEnvironmentVariable("DEVELOPMENT", "True");
            Environment.SetEnvironmentVariable("DISABLE_CLAMAV", "True");
            Environment.SetEnvironmentVariable("DB_CONNECTION_STRING", "Server=localhost;Port=3306;Database=tttdatabase;User=root;Password=testpassword");
            

            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IAuthenticationData));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }
                
                // Register singletons
                //Fake Data
                services.AddSingleton<IAuthenticationData, FakeAuthData>();
                services.AddSingleton<IAudioData, FakeAudioData>();
                services.AddSingleton<ISceneData, FakeSceneData>();
                services.AddSingleton<ISceneAudioData, FakeSceneAudioData>();
                
                //Debug Responses
                services.AddScoped<IHttpResponseConverter, DebugHttpResponseConverter>();
            });

            return base.CreateHost(builder);
        }
    }
}