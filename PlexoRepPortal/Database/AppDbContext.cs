using Microsoft.EntityFrameworkCore;
using PlexoRepPortal.Models;

namespace PlexoRepPortal.Database
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public virtual DbSet<Rep> Reps { get; set; } = null!;
        public virtual DbSet<RepDocument> RepDocuments { get; set; } = null!;
        public virtual DbSet<TrainingHubDocument> TrainingHubDocuments { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Rep>(entity =>
            {
                entity.ToTable("Reps");

                entity.HasKey(r => r.OId);

                entity.Property(r => r.RepId).HasMaxLength(30).IsRequired();
                entity.Property(r => r.FullName).HasMaxLength(200).IsRequired();
                entity.Property(r => r.Email).HasMaxLength(256).IsRequired();
                entity.Property(r => r.Phone).HasMaxLength(30);
                entity.Property(r => r.Address).HasMaxLength(300);
                entity.Property(r => r.City).HasMaxLength(100);
                entity.Property(r => r.State).HasMaxLength(50);
                entity.Property(r => r.Zip).HasMaxLength(20);
                entity.Property(r => r.GoogleLink).HasMaxLength(500);
                entity.Property(r => r.ResourceLink).HasMaxLength(500);
                entity.Property(r => r.Status).HasConversion<byte>().HasDefaultValue(RepStatus.Pending);
                entity.Property(r => r.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
                entity.Property(r => r.UpdatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

                entity.HasIndex(r => r.RepId).IsUnique();
            });

            modelBuilder.Entity<RepDocument>(entity =>
            {
                entity.ToTable("RepDocuments");

                entity.HasKey(d => d.OId);

                entity.Property(d => d.RepId).HasMaxLength(30).IsRequired();
                entity.Property(d => d.Kind).HasMaxLength(20).IsRequired();
                entity.Property(d => d.FileName).HasMaxLength(300).IsRequired();
                entity.Property(d => d.FilePath).HasMaxLength(500).IsRequired();
                entity.Property(d => d.UploadedAt).HasDefaultValueSql("SYSUTCDATETIME()");

                entity.HasOne<Rep>()
                    .WithMany()
                    .HasForeignKey(d => d.RepId)
                    .HasPrincipalKey(r => r.RepId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<TrainingHubDocument>(entity =>
            {
                entity.ToTable("TrainingHubDocuments");

                entity.HasKey(d => d.OId);

                entity.Property(d => d.RoleId).HasMaxLength(20).IsRequired();
                entity.Property(d => d.Title).HasMaxLength(200).IsRequired();
                entity.Property(d => d.Category).HasMaxLength(100);
                entity.Property(d => d.Description).HasMaxLength(1000);
                entity.Property(d => d.FileType).HasMaxLength(20).IsRequired();
                entity.Property(d => d.FileName).HasMaxLength(300).IsRequired();
                entity.Property(d => d.FilePath).HasMaxLength(500).IsRequired();
                entity.Property(d => d.Length).HasMaxLength(50);
                entity.Property(d => d.UploadedBy).HasMaxLength(10).HasDefaultValue("Rep").IsRequired();
                entity.Property(d => d.UploadedAt).HasDefaultValueSql("SYSUTCDATETIME()");

                entity.HasIndex(d => d.RoleId);
            });
        }
    }
}
