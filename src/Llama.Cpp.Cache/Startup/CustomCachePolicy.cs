using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Primitives;
using System.Text.Json.Nodes;

namespace Llama.Cpp.Cache.Startup
{
    public sealed class CustomCachePolicy(TimeSpan cacheDuration) : IOutputCachePolicy
    {
        async ValueTask IOutputCachePolicy.CacheRequestAsync(
            OutputCacheContext context,
            CancellationToken cancellationToken)
        {
            var attemptOutputCaching = AttemptOutputCaching(context);
            context.EnableOutputCaching = true;
            context.AllowLocking = true;

            if (attemptOutputCaching)
            {
                context.ResponseExpirationTimeSpan = cacheDuration;
                context.HttpContext.Request.EnableBuffering();
                var bodyStream = context.HttpContext.Request.Body;
                JsonNode? bodyJson = await JsonNode.ParseAsync(bodyStream, cancellationToken: cancellationToken);
                var streamFlag = bodyJson?["stream"]?.GetValue<bool>() ?? false;
                if (!streamFlag)
                {
                    string? json = bodyJson?.ToJsonString();// TODO: transform json to remove useless props for cache key
                    context.CacheVaryByRules.VaryByValues.Add("body", json);
                }
                else
                {
                    attemptOutputCaching = false;
                }
                
                // Reset the stream position to enable subsequent reads
                bodyStream.Position = 0;
            }

            context.AllowCacheLookup = attemptOutputCaching;
            context.AllowCacheStorage = attemptOutputCaching;
        }

        ValueTask IOutputCachePolicy.ServeFromCacheAsync
            (OutputCacheContext context, CancellationToken cancellationToken)
        {
            context.HttpContext.RequestServices.GetRequiredService<ILogger<CustomCachePolicy>>().LogInformation("Response comes from cache.");
            return ValueTask.CompletedTask;
        }

        ValueTask IOutputCachePolicy.ServeResponseAsync
            (OutputCacheContext context, CancellationToken cancellationToken)
        {
            var response = context.HttpContext.Response;

            // Verify existence of cookie headers
            if (!StringValues.IsNullOrEmpty(response.Headers.SetCookie))
            {
                context.AllowCacheStorage = false;
                return ValueTask.CompletedTask;
            }

            // Check response code
            if (response.StatusCode != StatusCodes.Status200OK &&
                response.StatusCode != StatusCodes.Status301MovedPermanently)
            {
                context.AllowCacheStorage = false;
                return ValueTask.CompletedTask;
            }

            if (context.AllowCacheStorage)
            {
                context.CacheVaryByRules.VaryByValues.TryGetValue("body", out var requestBody);
                context.HttpContext.RequestServices.GetRequiredService<ILogger<CustomCachePolicy>>().LogInformation("Caching response for:\r\n{RequestBody}", requestBody);
            }
            return ValueTask.CompletedTask;
        }

        private static bool AttemptOutputCaching(OutputCacheContext context)
        {
            var request = context.HttpContext.Request;

            if (!HttpMethods.IsPost(request.Method) ||
                request.ContentType?.Contains("application/json") != true)
            {
                return false;
            }

            return true;
        }
    }
}
