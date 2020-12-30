using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.Management.ResourceGraph.Models;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Newtonsoft.Json.Linq;

namespace AzCleaner.Func
{
    internal static class Extensions
    {
        public static bool IsDevelopment(this FunctionsHostBuilderContext context) =>
            StringComparer.OrdinalIgnoreCase.Equals(context.EnvironmentName, "Development");
        
        public static string GetDefaultSubscription(this ResourceManager.IAuthenticated authenticated) =>
            authenticated.Subscriptions.List().FirstOrDefault(s =>
                StringComparer.OrdinalIgnoreCase.Equals(s.State, "Enabled") ||
                StringComparer.OrdinalIgnoreCase.Equals(s.State, "Warned") ||
                StringComparer.OrdinalIgnoreCase.Equals(s.State, "PastDue"))?.SubscriptionId;

        public static IReadOnlyCollection<string> ToResources(this QueryResponse response) =>
            response.Count > 0
                ? JObject.FromObject(response.Data)["rows"]
                    .Select(x => x.First.ToString())
                    .ToList()
                : new List<string>();
    }
}