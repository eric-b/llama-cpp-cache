using Microsoft.Extensions.Options;

namespace Llama.Cpp.Cache.Components
{
    public sealed class FixedIncomingRequestTimeout(IOptions<FixedIncomingRequestTimeoutOptions> options, ILogger<FixedIncomingRequestTimeout> logger)
    {
        public CancellationTokenSource CreateCancellationTokenSource(HttpContext context)
        {
            if (options.Value.Timeout.HasValue)
            {
                logger.LogInformation("Overrides request aborted with fixed timeout of {Timeout}", options.Value.Timeout.Value);
                return new CancellationTokenSource(options.Value.Timeout.Value);
            }
            return CancellationTokenSource.CreateLinkedTokenSource(context.RequestAborted);
        }
    }
}
