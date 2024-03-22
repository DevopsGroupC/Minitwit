using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
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
            var watch = Stopwatch.StartNew(); // Start the stopwatch at the beginning of the request handling.

            ApplicationMetrics.HttpRequestTotal.Inc();
            // Before calling the next delegate in the pipeline, we don't do anything specific,
            // because we want to measure the whole processing time of the request.
            await _next(context);

            // Stop the stopwatch after the request has been fully processed.
            watch.Stop();

            // Record the elapsed time for the request in the histogram.
            ApplicationMetrics.HttpRequestDuration.Observe(watch.Elapsed.TotalSeconds);

            // If you want to log the request URL, it's recommended to do it before processing the request,
            // as logging at the end might not capture all details if an exception occurs.
            Console.WriteLine($"Request URL: {context.Request.Path}");
        }
    }
}
