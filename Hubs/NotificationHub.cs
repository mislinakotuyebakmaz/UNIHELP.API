using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace UniHelp.Api.Hubs;

public class NotificationHub : Hub
{
    // Bir kullanıcı SignalR'a bağlandığında bu metot çalışır.
    public override async Task OnConnectedAsync()
    {
        // Bağlantı isteğiyle birlikte gelen token'ı kullanarak kullanıcının ID'sini alıyoruz.
        // Bu, SignalR'ın kimlik doğrulama ile entegrasyonu sayesinde mümkün olur.
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!string.IsNullOrEmpty(userId))
        {
            // Her kullanıcıyı, kendi ID'sine özel bir gruba ekliyoruz.
            // Bu sayede, sadece o kullanıcıya özel bir mesaj gönderebiliriz.
            // Örnek: "user_2" adında bir grup, sadece ID'si 2 olan kullanıcıyı içerir.
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
        }

        await base.OnConnectedAsync();
    }

    // Kullanıcı bağlantısı koptuğunda bu metot çalışır.
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // (İsteğe bağlı) Kullanıcıyı gruptan çıkarmak için de benzer bir mantık kurulabilir.
        await base.OnDisconnectedAsync(exception);
    }
}