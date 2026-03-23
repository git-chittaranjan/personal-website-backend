using Serilog.Context;
using System.Diagnostics;

namespace my_api_app.Core.Middlewares.Logging
{
    public class CorrelationIdGenerator
    {
        private const string CorrelationHeader = "X-Correlation-Id";
        private readonly RequestDelegate _next;

        //Constructor
        public CorrelationIdGenerator(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context)
        {
            // Accept from upstream (API gateway, load balancer) or generate new
            var correlationId = context.Request.Headers[CorrelationHeader].FirstOrDefault()
                                ?? Guid.NewGuid().ToString("N");

            //context.TraceIdentifier is used by Kestrel. Kestral auto-generate one when the request hits Kestrel Server.
            //But the correlationId will replace that so that we will have single ID
            context.TraceIdentifier = correlationId;
            context.Items["CorrelationId"] = correlationId;
            context.Response.Headers[CorrelationHeader] = correlationId; //Client receives same correlation id in Header

            // Push into Serilog's LogContext so all downstream logs carry it
            using (LogContext.PushProperty("CorrelationId", correlationId)) //These Properties will appear in log file for {NewLine}{Properties:j} parameter
            using (LogContext.PushProperty("TraceIdentifier", context.TraceIdentifier))
            {
                await _next(context); //Inside 'using' so every log in that request will contain the same CorrelationId
            }
        }
    }
}
