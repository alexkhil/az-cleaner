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
        services.AddTransient<IAzRepository, AzV2Repository>();

        //services.AddAzureClients(x =>
        //{
        //    x.UseCredential(new DefaultAzureCredential());
        //    // x.ConfigureDefaults(context.Configuration.GetSection("AzureDefaults"));
        //});

        //services.AddSingleton(
        //    context.HostingEnvironment.IsDevelopment()
        //        ? SdkContext.AzureCredentialsFactory.FromFile("azureauth.json")
        //        : SdkContext.AzureCredentialsFactory.FromSystemAssignedManagedServiceIdentity(
        //            MSIResourceType.AppService, AzureEnvironment.AzureGlobalCloud));

        //services.AddScoped(s =>
        //{
        //    var credentials = s.GetRequiredService<AzureCredentials>();
        //    return ResourceManager.Configure().Authenticate(credentials);
        //});

        //services.AddScoped(s =>
        //{
        //    var authenticated = s.GetRequiredService<ResourceManager.IAuthenticated>();
        //    return authenticated.WithSubscription(authenticated.GetDefaultSubscription());
        //});

        //services.AddScoped<IResourceGraphClient>(s =>
        //    new ResourceGraphClient(
        //        s.GetRequiredService<AzureCredentials>(),
        //        s.GetRequiredService<IHttpClientFactory>().CreateClient(),
        //        disposeHttpClient: false));

        //services.AddScoped<AzRepository>();
        //services.AddScoped<IAzRepository>(s =>
        //    ActivatorUtilities.CreateInstance<ResilientAzRepository>(s, s.GetRequiredService<AzRepository>()));

        //services.AddSingleton<IAsyncPolicy>(_ =>
        //    Policy.Handle<Exception>()
        //        .WaitAndRetryAsync(new[] { TimeSpan.FromSeconds(2) + TimeSpan.FromMilliseconds(new Random().Next(0, 1000)) })
        //        .WithPolicyKey(PolicyNames.BasicRetry));

        //services.AddScoped<IAzCleaner, AzCleaner.Func.Domain.AzCleaner>();
    })
    .Build();

await host.RunAsync();