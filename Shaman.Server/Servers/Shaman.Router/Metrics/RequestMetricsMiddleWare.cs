using System;
using System.Linq;
using System.Threading.Tasks;
using App.Metrics;
using App.Metrics.Histogram;
using App.Metrics.Timer;
using Microsoft.AspNetCore.Http;

namespace Shaman.Router.Metrics
{
    public class RequestMetricsMiddleWare
    {
        private readonly RequestDelegate _next;
        private readonly IMetrics _metrics;

        private static readonly TimerOptions RequestTimeOptions = new TimerOptions
        {
            Name = "Request time",
            MeasurementUnit = Unit.Requests,
            DurationUnit = TimeUnit.Nanoseconds
        };

        public static readonly HistogramOptions RequestSizeHistogram = new HistogramOptions
        {
            Name = "Request body size",
            MeasurementUnit = Unit.Bytes
        };

        public static readonly HistogramOptions ResponseSizeHistogram = new HistogramOptions
        {
            Name = "Response body size",
            MeasurementUnit = Unit.Bytes
        };

        private static readonly string Path = "path";

        public RequestMetricsMiddleWare(RequestDelegate next, IMetrics metrics)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _metrics = metrics;
        }

        public async Task Invoke(HttpContext context)
        {
            var startTime = _metrics.Clock.Nanoseconds;
            var path = context.Request.Path;
            var httpMethod = context.Request.Method;
            
            var perEndpointMetricTags = new MetricTags(Path, path);

            if (httpMethod.Equals("POST", StringComparison.OrdinalIgnoreCase) ||
                httpMethod.Equals("PUT", StringComparison.OrdinalIgnoreCase))
            {
                if (context.Request.Headers != null && context.Request.Headers.ContainsKey("Content-Length"))
                {
                    var value = long.Parse(context.Request.Headers["Content-Length"].First());
                    _metrics.Measure.Histogram.Update(RequestSizeHistogram, value);
                    _metrics.Measure.Histogram.Update(RequestSizeHistogram, perEndpointMetricTags, value);
                }
            }

            await _next(context);

            
            var perEndpointtimer = _metrics.Provider.Timer.Instance(RequestTimeOptions, perEndpointMetricTags);
            perEndpointtimer.Record(_metrics.Clock.Nanoseconds - startTime, TimeUnit.Nanoseconds);
            
            var commonTimer = _metrics.Provider.Timer.Instance(RequestTimeOptions);
            commonTimer.Record(_metrics.Clock.Nanoseconds - startTime, TimeUnit.Nanoseconds);

            var responseContentLength = context.Response.ContentLength;
            if (responseContentLength.HasValue)
            {
                _metrics.Measure.Histogram.Update(ResponseSizeHistogram, perEndpointMetricTags,
                    responseContentLength.Value);
                _metrics.Measure.Histogram.Update(ResponseSizeHistogram, responseContentLength.Value);
            }
        }
    }
}