using Microsoft.AspNetCore.OutputCaching;

namespace Llama.Cpp.Cache.Startup
{
    static class OutputCacheConfiguration
    {
        public const string CustomPolicyName = "customPolicy";

        public static void Configure(OutputCacheOptions options, IConfigurationSection configuration)
        {
            var cacheDuration = configuration.GetValue("CacheDuration", TimeSpan.FromDays(1));
            options.AddPolicy(CustomPolicyName, new CustomCachePolicy(cacheDuration));
        }
    }
}
