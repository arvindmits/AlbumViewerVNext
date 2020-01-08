using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;

namespace AlbumViewerNetCore
{
    // Startup.cs: add this to ConfigureServices()
    // services.AddSingleton<ITelemetryInitializer, UserNameTelemetryInitializer>();
    public class UserNameTelemetryInitializer : ITelemetryInitializer
    {
        private IHttpContextAccessor HttpContextAccessor { get; }

        public UserNameTelemetryInitializer(IHttpContextAccessor httpContextAccessor)
        {
            HttpContextAccessor = httpContextAccessor;
        }

        public void Initialize(ITelemetry telemetry)
        {
            var requestTelemetry = telemetry as RequestTelemetry;
            if (requestTelemetry == null)
            {
                return;
            }

            var nameClaimValue = HttpContextAccessor.HttpContext?.User?.Identity?.Name;
            if (string.IsNullOrEmpty(nameClaimValue))
            {
                return;
            }

            requestTelemetry.Properties["UserName"] = nameClaimValue;
        }
    }
}
