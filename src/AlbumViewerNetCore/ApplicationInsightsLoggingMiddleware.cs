using System.IO;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Http;

namespace AlbumViewerNetCore
{
    // Startup.cs: add this to Configure()
    // app.UseMiddleware<ApplicationInsightsLoggingMiddleware>();
    public class ApplicationInsightsLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private TelemetryClient TelemetryClient { get; }

        public ApplicationInsightsLoggingMiddleware(RequestDelegate next, TelemetryClient telemetryClient)
        {
            _next = next;
            TelemetryClient = telemetryClient;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Inbound (before the controller)
            var request = context?.Request;
            if (request == null)
            {
                await _next(context);
                return;
            }

            request.EnableBuffering();  // Allows us to reuse the existing Request.Body

            var requestBody = new MemoryStream();
            await request.Body.CopyToAsync(requestBody).ConfigureAwait(false);
            request.Body.Position = 0;

            // Swap the original Response.Body stream with one we can read / seek
            var originalResponseBody = context.Response.Body;
            using var replacementResponseBody = new MemoryStream();
            context.Response.Body = replacementResponseBody;

            await _next(context); // Continue processing (additional middleware, controller, etc.)

            // Outbound (after the controller)
            replacementResponseBody.Position = 0;

            // Copy the response body to the original stream
            await replacementResponseBody.CopyToAsync(originalResponseBody).ConfigureAwait(false);
            context.Response.Body = originalResponseBody;

            var response = context.Response;
            if (response.StatusCode < 400)
            {
                return;
            }

            var requestTelemetry = context.Features.Get<RequestTelemetry>();
            if (requestTelemetry == null)
            {
                return;
            }

            if (requestBody.CanRead)
            {
                var requestBodyString = await ReadBodyStream(requestBody).ConfigureAwait(false);
                requestTelemetry.Properties.Add("RequestBody", requestBodyString);  // limit: 8192 characters
                TelemetryClient.TrackTrace(requestBodyString);
            }

            if (replacementResponseBody.CanRead)
            {
                var responseBodyString = await ReadBodyStream(replacementResponseBody).ConfigureAwait(false);
                requestTelemetry.Properties.Add("ResponseBody", responseBodyString);
                TelemetryClient.TrackTrace(responseBodyString);
            }
        }

        private async Task<string> ReadBodyStream(Stream body)
        {
            if (body.Length == 0)
            {
                return null;
            }

            body.Position = 0;

            using var reader = new StreamReader(body, leaveOpen: true);
            var bodyString = await reader.ReadToEndAsync().ConfigureAwait(false);
            body.Position = 0;

            return bodyString;
        }
    }
}
