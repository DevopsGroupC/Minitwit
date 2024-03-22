using Prometheus;

public static class ApplicationMetrics
{
    public static readonly Histogram HttpRequestDuration = Metrics
        .CreateHistogram("minitwit_http_request_duration_seconds", "Histogram of HTTP request duration.",
            new HistogramConfiguration
            {
                // Define buckets with appropriate ranges for your application
                Buckets = Histogram.LinearBuckets(start: 0.1, width: 0.1, count: 10)
            });

    public static readonly Counter HttpRequestTotal = Metrics
        .CreateCounter("minitwit_http_requests_total", "Total number of HTTP requests made to the application.");
}
