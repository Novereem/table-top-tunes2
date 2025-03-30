using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Shared.Interfaces.Data;
using Shared.Interfaces.Services;
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
            Environment.SetEnvironmentVariable("DB_CONNECTION_STRING", "Server=localhost;Port=3306;Database=tttdatabase;User=root;Password=testpassword");

            builder.ConfigureServices(services =>
            {
                // Remove the real IAuthenticationData registration if it exists.
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IAuthenticationData));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Register FakeAuthData as a singleton for the entire test run.
                services.AddSingleton<IAuthenticationData, FakeAuthData>();
            });

            return base.CreateHost(builder);
        }
    }
}