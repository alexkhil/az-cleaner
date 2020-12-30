using System;
using System.Net.Http;
using AzCleaner.Domain;
using AzCleaner.Func.Repositories;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.Management.ResourceGraph;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Polly;

[assembly: FunctionsStartup(typeof(AzCleaner.Func.Startup))]

namespace AzCleaner.Func
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddHttpClient();

            builder.Services.AddSingleton(
                builder.GetContext().IsDevelopment()
                ? SdkContext.AzureCredentialsFactory.FromFile("azureauth.json")
                : SdkContext.AzureCredentialsFactory.FromSystemAssignedManagedServiceIdentity(
                    MSIResourceType.AppService, AzureEnvironment.AzureGlobalCloud));

            builder.Services.AddScoped(s =>
            {
                var credentials = s.GetRequiredService<AzureCredentials>();
                return ResourceManager.Configure().Authenticate(credentials);
            });

            builder.Services.AddScoped(s =>
            {
                var authenticated = s.GetRequiredService<ResourceManager.IAuthenticated>();
                return authenticated.WithSubscription(authenticated.GetDefaultSubscription());
            });

            builder.Services.AddScoped<IResourceGraphClient>(s =>
                new ResourceGraphClient(
                    s.GetRequiredService<AzureCredentials>(), 
                    s.GetRequiredService<IHttpClientFactory>().CreateClient(),
                    disposeHttpClient: false));

            builder.Services.AddScoped<AzRepository>();
            builder.Services.AddScoped<IAzRepository>(s =>
                ActivatorUtilities.CreateInstance<ResilientAzRepository>(s, s.GetRequiredService<AzRepository>()));

            builder.Services.AddSingleton<IAsyncPolicy>(_ =>
                Policy.Handle<Exception>()
                .WaitAndRetryAsync(new[] {TimeSpan.FromSeconds(2) + TimeSpan.FromMilliseconds(new Random().Next(0, 1000))})
                .WithPolicyKey(PolicyNames.BasicRetry));

            builder.Services.AddScoped<IAzCleaner, Domain.AzCleaner>();
        }
    }
}