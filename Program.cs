using Serilog;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using UniHelp.Api.Data;
using UniHelp.Api.Hubs;
using UniHelp.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Serilog yapılandırması
builder.Host.UseSerilog((context, configuration) => 
    configuration.ReadFrom.Configuration(context.Configuration));

try
{
    // --- SERVİSLERİ EKLEME BÖLÜMÜ ---
    
    builder.Services.AddDbContext<DataContext>(options =>
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
        options.UseLazyLoadingProxies();
    });

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.AddSignalR(); 

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                    builder.Configuration.GetSection("AppSettings:Token").Value!)),
                ValidateIssuer = false,
                ValidateAudience = false
            };
        });

    // === GÜNCELLENMİŞ CORS POLİTİKASI ===
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowSpecificOrigin",
            policy =>
            {
                // Test için daha esnek hale getiriyoruz: Herhangi bir kaynaktan gelen isteğe izin ver.
                policy.AllowAnyOrigin() 
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
    });
    // ===================================

    var app = builder.Build();
    
    // --- MIDDLEWARE PİPELINE'INI YAPILANDIRMA ---

    app.UseSerilogRequestLogging();
    app.UseMiddleware<ExceptionMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }
    
    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseRouting();

    // CORS middleware'ini çağırıyoruz
    app.UseCors("AllowSpecificOrigin");
    
    app.UseAuthentication();
    app.UseAuthorization();
    
    app.MapHub<NotificationHub>("/notificationHub");
    app.MapControllers();

    Log.Information("Uygulama başlatılıyor...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Uygulama başlatılırken kritik bir hata oluştu.", ex);
}
finally
{
    Log.CloseAndFlush();
}