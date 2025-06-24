using Serilog;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters; // Bu kütüphane artık AddSecurityRequirement ile değiştirildiği için zorunlu değil
using System.Text;
using UniHelp.Api.Data;
using UniHelp.Api.Hubs;
using UniHelp.Api.Middleware;
using System.Reflection;
using UniHelp.Api.Services;
using UniHelp.Api.Repositories;

// --- builder ve Serilog yapılandırması aynı kalıyor ---
var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

try
{
    // --- SERVİSLERİ EKLEME BÖLÜMÜ ---
    
    // DbContext, Controllers, SignalR yapılandırmaları aynı kalıyor
    builder.Services.AddDbContext<DataContext>(options =>
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    });
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSignalR();

    // === DEĞİŞİKLİK 1: GÜNCELLENMİŞ VE STANDART SWAGGERGEN YAPILANDIRMASI ===
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "UniHelp API",
            Version = "v1",
            Description = "UniHelp projesi için REST API",
            Contact = new OpenApiContact
            {
                Name = "UniHelp Team",
                Email = "support@unihelp.com"
            }
        });

        // "Authorize" butonunu ve güvenlik şemasını doğru şekilde tanımlıyoruz
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization. 'Bearer' kelimesini yazmadan SADECE token'ı girin.",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http, // ApiKey yerine Http kullanılmalı
            Scheme = "bearer", // şema adı küçük harfle standarttır
            BearerFormat = "JWT"
        });

        // OperationFilter yerine, bu şemanın kullanılmasını zorunlu kılan kuralı ekliyoruz
        // Bu, tüm yetki gerektiren endpoint'lere kilit ikonu ekler ve token'ı doğru formatta gönderir.
        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });

        // XML yorumları için kodun aynı kalıyor
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            options.IncludeXmlComments(xmlPath);
        }
    });
    
    // Authentication yapılandırman doğru, aynı kalabilir
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

    // === DEĞİŞİKLİK 2: DAHA ESNEK CORS POLİTİKASI (GELİŞTİRME ORTAMI İÇİN) ===
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", // Politika adını daha genel yapabiliriz
            policy =>
            {
                policy.AllowAnyOrigin()  // Belirli bir origin yerine tümüne izin ver (geliştirme için ideal)
                      .AllowAnyHeader()
                      .AllowAnyMethod();
                // NOT: AllowAnyOrigin() ile AllowCredentials() aynı anda kullanılamaz.
                // SignalR için eğer creds gerekiyorsa, WithOrigins("http://senin-frontend-adresin") kullanıp
                // AllowCredentials() ekleyebilirsin. Şimdilik API testi için bu en iyisi.
            });
    });

    // Repository ve Service kayıtların aynı kalıyor
    builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
    builder.Services.AddScoped<IQuestionService, QuestionService>();

    var app = builder.Build();
    
    // --- MIDDLEWARE PİPELİNE'INI YAPILANDIRMA ---

    app.UseSerilogRequestLogging();
    app.UseMiddleware<ExceptionMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "UniHelp API v1");
            c.RoutePrefix = "swagger";
            c.DocumentTitle = "UniHelp API Documentation";
            c.DefaultModelsExpandDepth(-1);
        });
    }
    
    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseRouting();

    // === DEĞİŞİKLİK 3: YENİ CORS POLİTİKASINI UYGULA ===
    app.UseCors("AllowAll"); // Yeni, daha esnek politikanın adını buraya yaz

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