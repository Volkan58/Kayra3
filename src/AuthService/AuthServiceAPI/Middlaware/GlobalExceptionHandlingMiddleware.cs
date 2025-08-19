using System.Net;
using System.Text.Json;

namespace AuthServiceAPI.Middlaware
{
    public class GlobalExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

        public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Beklenmeyen hata oluştu");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var response = new
            {
                error = new
                {
                    message = "Bir hata oluştu",
                    details = exception.Message,
                    timestamp = DateTime.UtcNow,
                    path = context.Request.Path,
                    method = context.Request.Method
                }
            };

            var (statusCode, message) = exception switch
            {
                ArgumentException => (HttpStatusCode.BadRequest, "Geçersiz parametre"),
                UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Yetkilendirme gerekli"),
                InvalidOperationException => (HttpStatusCode.BadRequest, "Geçersiz işlem"),
                KeyNotFoundException => (HttpStatusCode.NotFound, "Kayıt bulunamadı"),
                _ => (HttpStatusCode.InternalServerError, "Sunucu hatası")
            };

            context.Response.StatusCode = (int)statusCode;
            response = new
            {
                error = new
                {
                    message = message,
                    details = exception.Message,
                    timestamp = DateTime.UtcNow,
                    path = context.Request.Path,
                    method = context.Request.Method
                }
            };

            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(jsonResponse);
        }
    }
}
