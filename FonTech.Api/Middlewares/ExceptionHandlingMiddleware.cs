using FonTech.Domain.Enum;
using FonTech.Domain.Result;
using ILogger = Serilog.ILogger;

namespace FonTech.Api.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext httpContext, Exception exception)
        {
            _logger.Error(exception, exception.Message);

            var errorMessage = exception.Message;
            var response = exception switch //интересная конструкция, загуглить че это подробнее
            {
                UnauthorizedAccessException _ => new BaseResult() { ErrorMessage = exception.Message, ErrorCode = (int)ErrorCodes.UserUnauthorizedAccess },
                _ => new BaseResult() { ErrorMessage = "Internal server error. Please retry later.", ErrorCode = (int)ErrorCodes.InternalServerError }
            };

            httpContext.Response.ContentType = "application/json";
            httpContext.Response.StatusCode = (int)response.ErrorCode;

            await httpContext.Response.WriteAsJsonAsync(response);
        }

    }
}
