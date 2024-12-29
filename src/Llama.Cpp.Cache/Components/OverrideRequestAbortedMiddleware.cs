
namespace Llama.Cpp.Cache.Components
{
    public sealed class OverrideRequestAbortedMiddleware(FixedIncomingRequestTimeout fixedTimeout, RequestDelegate next, ILogger<OverrideRequestAbortedMiddleware> logger)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            using var cts = fixedTimeout.CreateCancellationTokenSource(context);
            var originalClientCancellation = context.RequestAborted;
            context.RequestAborted = cts.Token;
            await next(context);
            if (originalClientCancellation.IsCancellationRequested)
            {
                logger.LogInformation("Request ended after client aborted it.");
            }
            context.RequestAborted = originalClientCancellation;
        }
    }

    public static class OverrideRequestAbortedMiddlewareExtensions
    {
        public static IApplicationBuilder UseOverrideRequestAborted(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<OverrideRequestAbortedMiddleware>();
        }
    }
}
