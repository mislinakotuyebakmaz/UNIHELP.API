using Microsoft.EntityFrameworkCore;
using UniHelp.Api.Entities;

namespace UniHelp.Api.Data;

public class DataContext : DbContext
{
    // Bu constructor, uygulama normal çalışırken Dependency Injection tarafından kullanılır.
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }
    
    // Veritabanında oluşturulacak tabloları burada tanımlıyoruz.
    public DbSet<User> Users { get; set; }
    public DbSet<Note> Notes { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<Answer> Answers { get; set; }
}