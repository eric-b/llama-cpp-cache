using Yarp.ReverseProxy.Configuration;

namespace Llama.Cpp.Cache.Startup
{
    static class ReverseProxyConfiguration
    {
        private const string SingleClusterId = "single-cluster";

        public static RouteConfig[] GetRoutes()
        {
            return
            [
                new RouteConfig()
                {
                    RouteId = "catch-all-route",
                    ClusterId = SingleClusterId,
                    Match = new RouteMatch { Path = "{**catch-all}" },
                    OutputCachePolicy = OutputCacheConfiguration.CustomPolicyName
                }
            ];
        }

        public static ClusterConfig[] GetClusters(IConfigurationSection upstreamConfigurationSection)
        {
            // TODO: if caller is a browser, direct proxy without buffering, without in memory cache, with session affinity, to maintain chat context.
            // All other calls (API client) are buffered and cached, without incoming cancellation token.
            // RequestTransformContext  ?

            var upstreamUrl = upstreamConfigurationSection.GetValue<string>("Url");
            if (string.IsNullOrEmpty(upstreamUrl))
                throw new ArgumentException($"Missing application setting 'Url' in section {upstreamConfigurationSection.Path}.");
            var upstreamTimeout = upstreamConfigurationSection.GetValue<TimeSpan>("Timeout", TimeSpan.FromMinutes(10));
            if (!upstreamUrl.EndsWith('/'))
                upstreamUrl += '/';

            return
            [
                new ClusterConfig()
                {
                    ClusterId = SingleClusterId,
                    Destinations = new Dictionary<string, DestinationConfig>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "single-destination", new DestinationConfig() { Address = upstreamUrl } }
                    }, 
                    HttpRequest = new Yarp.ReverseProxy.Forwarder.ForwarderRequestConfig
                    {
                        ActivityTimeout = upstreamTimeout,
                        // TODO: should be context aware to enable buffering only for cachable requests (not from browser with streaming enabled)
                        AllowResponseBuffering = true
                    }
                }
            ];
        }
    }
}
