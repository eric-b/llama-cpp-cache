using Microsoft.AspNetCore.OutputCaching;

namespace Llama.Cpp.Cache.Startup
{
    static class OutputCacheConfiguration
    {
        public const string CustomPolicyName = "customPolicy";

        public static void Configure(OutputCacheOptions options, IConfigurationSection upstreamConfigurationSection)
        {
            var cacheDuration = upstreamConfigurationSection.GetValue("Timeout", TimeSpan.FromSeconds(100)) * 2;
            options.AddPolicy(CustomPolicyName, new CustomCachePolicy(cacheDuration));
        }
    }
}
