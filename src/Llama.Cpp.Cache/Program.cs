using Llama.Cpp.Cache.Components;
using Llama.Cpp.Cache.Startup;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;
services.AddSingleton<FixedIncomingRequestTimeout>();
services.Configure<FixedIncomingRequestTimeoutOptions>(builder.Configuration.GetSection("Upstream"));

services.AddOutputCache(options => OutputCacheConfiguration.Configure(options, builder.Configuration.GetSection("OutputCache")));

services.AddReverseProxy().LoadFromMemory(
    ReverseProxyConfiguration.GetRoutes(), 
    ReverseProxyConfiguration.GetClusters(builder.Configuration.GetSection("Upstream")));

var app = builder.Build();

app.UseOverrideRequestAborted();
app.UseOutputCache();
app.MapReverseProxy();

app.Run();