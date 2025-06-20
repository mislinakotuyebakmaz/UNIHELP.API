using System.Net;
using System.Text.Json;

namespace UniHelp.Api.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Bir sonraki middleware'i çağır. Eğer hata olmazsa, işlem burada biter.
            await _next(context);
        }
        catch (Exception ex)
        {
            // Eğer bir hata yakalanırsa, logla ve kullanıcıya standart bir yanıt döndür.
            _logger.LogError(ex, ex.Message);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError; // 500 hatası

            var response = _env.IsDevelopment()
    // Geliştirme ortamındaysak, hatanın detaylarını göster (debugging için).
    ? new 
      { 
          statusCode = context.Response.StatusCode, 
          message = ex.Message, 
          details = ex.StackTrace?.ToString() 
      }
    // Üretim (production) ortamındaysak, detayları null olarak ayarla.
    : new 
      { 
          statusCode = context.Response.StatusCode, 
          message = "Internal Server Error",
          details = (string?)null // <-- DÜZELTME BURADA
      };
            
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var json = JsonSerializer.Serialize(response, options);

            await context.Response.WriteAsync(json);
        }
    }
}