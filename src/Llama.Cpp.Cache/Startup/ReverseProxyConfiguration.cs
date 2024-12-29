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
            var upstreamUrl = upstreamConfigurationSection.GetValue<string>("Url");
            if (string.IsNullOrEmpty(upstreamUrl))
                throw new ArgumentException($"Missing application setting 'Url' in section {upstreamConfigurationSection.Path}.");
            var upstreamTimeout = upstreamConfigurationSection.GetValue<TimeSpan>("Timeout");
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
                        ActivityTimeout = upstreamTimeout
                    }
                }
            ];
        }
    }
}
