
using Document.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Document.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Department> Departments => Set<Department>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Document.Api.Models.Document> Documents
        => Set<Document.Api.Models.Document>();
    public DbSet<DocumentHistory> DocumentHistories
        => Set<DocumentHistory>();
    public DbSet<Notification> Notifications
        => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Department>(entity =>
        {
            entity.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(x => x.FullName)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(x => x.Role)
                .IsRequired()
                .HasMaxLength(30);

            entity.HasOne<Department>()
                .WithMany()
                .HasForeignKey(x => x.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Document.Api.Models.Document>(entity =>
        {
            entity.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(x => x.DocumentType)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(x => x.Content)
                .IsRequired();

            entity.Property(x => x.Status)
                .IsRequired()
                .HasMaxLength(20);

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.ApproverId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.Status);
            entity.HasIndex(x => x.ApproverId);
        });

        modelBuilder.Entity<DocumentHistory>(entity =>
        {
            entity.Property(x => x.Action)
                .IsRequired()
                .HasMaxLength(30);

            entity.HasOne<Document.Api.Models.Document>()
                .WithMany()
                .HasForeignKey(x => x.DocumentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.PerformedById)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.Property(x => x.Message)
                .IsRequired();

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Document.Api.Models.Document>()
                .WithMany()
                .HasForeignKey(x => x.DocumentId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
