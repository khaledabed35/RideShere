using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace BLL.Error
{
    public class GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger)
        : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            logger.LogError(
                exception,
                "Unhandled Exception Occurred: {Message}",
                exception.Message);

            int statusCode = exception switch
            {
                BadRequestException => StatusCodes.Status400BadRequest,
                UnauthorizedException => StatusCodes.Status401Unauthorized,
                NotFoundException => StatusCodes.Status404NotFound,
                _ => StatusCodes.Status500InternalServerError
            };

            string message = statusCode == StatusCodes.Status500InternalServerError
                ? "Internal Server Error"
                : exception.Message;

            var response = new ApiResponse(statusCode, message);

            httpContext.Response.StatusCode = statusCode;
            httpContext.Response.ContentType = "application/json";

            await httpContext.Response.WriteAsJsonAsync(
                response,
                cancellationToken);

            return true;
        }
    }
}