using System.Net;

namespace WebAPI.Middleware
{
    public class ExceptionLoggingMiddleware
    {
        private readonly ILogger<ExceptionLoggingMiddleware> _logger;
        private readonly RequestDelegate _requestDelegate;

        public ExceptionLoggingMiddleware(ILogger<ExceptionLoggingMiddleware> logger, RequestDelegate requestDelegate)
        {
            _logger = logger;
            _requestDelegate = requestDelegate;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _requestDelegate(httpContext);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception has occurred.");
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var result = new
            {
                StatusCode = context.Response.StatusCode,
                Message = "Internal Server Error. Please try again later.",
                Detailed = exception.Message
            };

            return context.Response.WriteAsJsonAsync(result);
        }
    }
}
