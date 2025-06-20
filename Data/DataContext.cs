using Microsoft.EntityFrameworkCore;
using UniHelp.Api.Entities;

namespace UniHelp.Api.Data;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Note> Notes { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<Answer> Answers { get; set; }

    // === BU METODU EKLE ===
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User -> Answer ilişkisi için kural belirliyoruz.
        modelBuilder.Entity<Answer>()
            .HasOne(a => a.User) // Bir Cevabın bir Kullanıcısı vardır
            .WithMany(u => u.Answers) // Bir Kullanıcının çok Cevabı vardır
            .HasForeignKey(a => a.UserId) // Yabancı anahtar UserId'dir
            .OnDelete(DeleteBehavior.NoAction); // ÖNEMLİ: User silinirse, bu ilişki üzerinden bir şey yapma (NoAction)

        // Question -> Answer ilişkisi için kural belirliyoruz (Bu zaten varsayılan olduğu için zorunlu değil ama açıkça belirtmek iyidir).
        modelBuilder.Entity<Answer>()
            .HasOne(a => a.Question)
            .WithMany(q => q.Answers)
            .HasForeignKey(a => a.QuestionId)
            .OnDelete(DeleteBehavior.Cascade); // Question silinirse, cevapları da sil (Cascade)
    }
    // ======================
}