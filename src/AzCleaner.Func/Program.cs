using AzCleaner.Func.DataAccess;
using AzCleaner.Func.Domain;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        services.AddSingleton<TokenCredential, AzureCliCredential>();
        services.AddSingleton<ArmClient>();
        services.AddTransient<IAzRepository, AzRepository>();
        services.AddTransient<IAzCleaner, AzCleaner.Func.Domain.AzCleaner>();
    })
    .Build();

await host.RunAsync();