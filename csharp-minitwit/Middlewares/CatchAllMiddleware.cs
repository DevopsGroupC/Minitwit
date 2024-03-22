using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using csharp_minitwit.Utils;
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

            ApplicationMetrics.HttpRequestTotal.WithLabels(MetricsHelpers.SanitizePath(context.Request.Path)).Inc();

            await _next(context);

            watch.Stop();

            ApplicationMetrics.HttpRequestDuration
                .WithLabels(MetricsHelpers.SanitizePath(context.Request.Path))
                .Observe(watch.Elapsed.TotalSeconds);

            ApplicationMetrics.HttpResponseStatusCodeTotal.WithLabels(context.Response.StatusCode.ToString()).Inc();

            Console.WriteLine(context.Response.StatusCode);
        }
    }
}
