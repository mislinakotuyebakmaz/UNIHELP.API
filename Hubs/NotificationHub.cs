using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace UniHelp.Api.Hubs;

[Authorize] // Bu önemli - Hub'ın kimlik doğrulaması gerekiyor
public class NotificationHub : Hub
{
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(ILogger<NotificationHub> logger)
    {
        _logger = logger;
    }

    // Bir kullanıcı SignalR'a bağlandığında bu metot çalışır.
    public override async Task OnConnectedAsync()
    {
        // Bağlantı isteğiyle birlikte gelen token'ı kullanarak kullanıcının ID'sini alıyoruz.
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var username = Context.User?.FindFirst(ClaimTypes.Name)?.Value;

        _logger.LogInformation("SignalR bağlantı denemesi - ConnectionId: {ConnectionId}", Context.ConnectionId);
        _logger.LogInformation("Context.User null mu? {IsNull}", Context.User == null);
        
        if (Context.User != null)
        {
            _logger.LogInformation("User Claims: {Claims}", 
                string.Join(", ", Context.User.Claims.Select(c => $"{c.Type}={c.Value}")));
        }

        if (!string.IsNullOrEmpty(userId))
        {
            var groupName = $"user_{userId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            
            _logger.LogInformation("✅ Kullanıcı başarıyla bağlandı - Username: {Username}, UserId: {UserId}, Group: {GroupName}, ConnectionId: {ConnectionId}", 
                username, userId, groupName, Context.ConnectionId);
        }
        else
        {
            _logger.LogWarning("❌ SignalR bağlantısında kullanıcı ID'si bulunamadı! ConnectionId: {ConnectionId}", Context.ConnectionId);
            _logger.LogWarning("Token doğrulama problemi olabilir. Context.User: {User}", Context.User?.Identity?.Name);
        }

        await base.OnConnectedAsync();
    }

    // Kullanıcı bağlantısı koptuğunda bu metot çalışır.
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var username = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
        
        if (!string.IsNullOrEmpty(userId))
        {
            var groupName = $"user_{userId}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            
            _logger.LogInformation("❌ Kullanıcı bağlantısı kesildi - Username: {Username}, UserId: {UserId}, Group: {GroupName}, ConnectionId: {ConnectionId}", 
                username, userId, groupName, Context.ConnectionId);
        }

        if (exception != null)
        {
            _logger.LogError(exception, "SignalR bağlantısı hata ile kesildi - ConnectionId: {ConnectionId}", Context.ConnectionId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    // Test amaçlı metot - frontend'den çağırılabilir
    public async Task SendTestMessage(string message)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var username = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
        
        _logger.LogInformation("Test mesajı alındı - Username: {Username}, UserId: {UserId}, Message: {Message}", 
            username, userId, message);
            
        // Sadece gönderen kullanıcıya geri gönder
        await Clients.Caller.SendAsync("ReceiveNotification", $"Test mesajınız alındı: {message}");
    }

    // Kullanıcının kendi grubuna mesaj gönder
    public async Task SendToMyself(string message)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (!string.IsNullOrEmpty(userId))
        {
            var groupName = $"user_{userId}";
            await Clients.Group(groupName).SendAsync("ReceiveNotification", $"Kendinize mesaj: {message}");
            _logger.LogInformation("Kullanıcı kendine mesaj gönderdi - UserId: {UserId}, Message: {Message}", userId, message);
        }
    }
}
