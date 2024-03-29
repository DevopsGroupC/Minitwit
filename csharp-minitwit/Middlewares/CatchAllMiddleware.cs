using System.Diagnostics;
using System.Threading.Tasks;

using csharp_minitwit.Utils;

using Microsoft.AspNetCore.Http;
// Assuming ApplicationMetrics is in the same namespace, or add the appropriate using statement

namespace csharp_minitwit.Middlewares
{
    public class CatchAllMiddleware
    {
        private readonly RequestDelegate _next;

        public CatchAllMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var watch = Stopwatch.StartNew();

            // Used to monitor total requests received grouped by endpoint
            ApplicationMetrics.HttpRequestTotal.WithLabels(MetricsHelpers.SanitizePath(context.Request.Path)).Inc();

            await _next(context);

            watch.Stop();
            // Used to monitor response delay grouped by endpoint
            ApplicationMetrics.HttpRequestDuration
                .WithLabels(MetricsHelpers.SanitizePath(context.Request.Path))
                .Observe(watch.Elapsed.TotalSeconds);

            // Used to monitor response status codes grouped by endpoint
            ApplicationMetrics.HttpResponseStatusCodeTotal.WithLabels(context.Response.StatusCode.ToString()).Inc();
        }
    }
}