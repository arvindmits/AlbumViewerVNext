using System.IO;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Http;

namespace AlbumViewerNetCore
{
    public class ApplicationInsightsLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public ApplicationInsightsLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var request = context?.Request;
            if (request == null)
            {
                await _next(context);
                return;
            }

            request.EnableBuffering();

            var existingBody = context.Response.Body;
            using var responseBodyStream = new MemoryStream();
            context.Response.Body = responseBodyStream;

            await _next(context);

            responseBodyStream.Position = 0;
            await responseBodyStream.CopyToAsync(existingBody).ConfigureAwait(false);
            context.Response.Body = existingBody;
            responseBodyStream.Position = 0;

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

            if (request.Body.CanRead)
            {
                var requestBodyString = await ReadBodyStream(request.Body).ConfigureAwait(false);
                requestTelemetry.Properties.Add("RequestBody", requestBodyString);
            }

            if (responseBodyStream.CanRead)
            {
                var responseBodyString = await ReadBodyStream(responseBodyStream).ConfigureAwait(false);
                requestTelemetry.Properties.Add("ResponseBody", responseBodyString);
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
