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
        public virtual DbSet<RepBankDetails> RepBankDetails { get; set; } = null!;
        public virtual DbSet<TrainingHubLink> TrainingHubLinks { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Rep>(entity =>
            {
                entity.ToTable("Reps");

                entity.HasKey(r => r.OId);

                entity.Property(r => r.RepId).HasMaxLength(30).IsRequired();
                entity.Property(r => r.FullName).HasMaxLength(200).IsRequired();
                entity.Property(r => r.BusinessName).HasMaxLength(200);
                entity.Property(r => r.Email).HasMaxLength(256).IsRequired();
                entity.Property(r => r.Phone).HasMaxLength(30);
                entity.Property(r => r.SalesRepType).HasConversion<byte>().HasDefaultValue(SalesRepType.ReferralAgent);
                entity.Property(r => r.Address).HasMaxLength(300);
                entity.Property(r => r.City).HasMaxLength(100);
                entity.Property(r => r.State).HasMaxLength(50);
                entity.Property(r => r.Zip).HasMaxLength(20);
                entity.Property(r => r.GoogleLink).HasMaxLength(500);
                entity.Property(r => r.ResourceLink).HasMaxLength(500);
                entity.Property(r => r.PricingSheetLink).HasMaxLength(500);
                entity.Property(r => r.PowerPointLink).HasMaxLength(500);
                entity.Property(r => r.Status).HasConversion<byte>().HasDefaultValue(RepStatus.Pending);
                entity.Property(r => r.PassedCertification).HasDefaultValue(false);
                entity.Property(r => r.BusinessCardsSent).HasDefaultValue(false);
                entity.Property(r => r.ConsultantFeePaid).HasDefaultValue(false);
                entity.Property(r => r.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
                entity.Property(r => r.UpdatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
                // Soft-delete flag — stored as a BIT (0/1), surfaced in code as DeleteStatus so callers
                // can't accidentally treat it as a generic bool. Rows are never physically removed;
                // RepsController's Delete endpoint just flips this instead (see DeleteStatus.cs).
                // Deliberately NOT using .HasDefaultValue() here: EF Core treats a property whose
                // current value equals the CLR default (NotDeleted/false) as "unset" and would omit
                // it from the INSERT, letting the column's own SQL DEFAULT apply instead — since that
                // default is 1 (Deleted), every API-created rep would otherwise come back invisible.
                // Leaving ValueGenerated at its normal Never means every insert writes the real value.
                entity.Property(r => r.Delete)
                    .HasConversion(v => v == DeleteStatus.Deleted, v => v ? DeleteStatus.Deleted : DeleteStatus.NotDeleted);

                entity.Property(r => r.ContractWizardLink).HasMaxLength(500);
                entity.Property(r => r.ContractWizardUsername).HasMaxLength(200);
                // Ciphertext (AES) — sized generously since encrypted output is longer than the plaintext it holds.
                entity.Property(r => r.ContractWizardPassword).HasMaxLength(500);
                entity.Property(r => r.ContractWizardInstructionsLink).HasMaxLength(500);
                entity.Property(r => r.PwrRewardsEmail).HasMaxLength(256);
                entity.Property(r => r.PwrRewardsEmailPassword).HasMaxLength(500);

                entity.HasIndex(r => r.RepId).IsUnique();

                // Soft-deleted reps stay invisible to every query against this DbSet (GetAll, GetById,
                // Update, ValidateRepId, ...) without each caller needing to filter it out itself.
                entity.HasQueryFilter(r => r.Delete == DeleteStatus.NotDeleted);
            });

            modelBuilder.Entity<RepBankDetails>(entity =>
            {
                entity.ToTable("RepBankDetails");

                entity.HasKey(b => b.OId);

                entity.Property(b => b.RepId).HasMaxLength(30).IsRequired();
                // Ciphertext (AES) — sized generously since encrypted output is longer than the plaintext it holds.
                entity.Property(b => b.BankName).HasMaxLength(500).IsRequired();
                entity.Property(b => b.RoutingNumber).HasMaxLength(500).IsRequired();
                entity.Property(b => b.AccountNumber).HasMaxLength(500).IsRequired();
                entity.Property(b => b.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
                entity.Property(b => b.UpdatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

                entity.HasIndex(b => b.RepId).IsUnique();

                entity.HasOne<Rep>()
                    .WithMany()
                    .HasForeignKey(b => b.RepId)
                    .HasPrincipalKey(r => r.RepId)
                    .OnDelete(DeleteBehavior.Cascade);
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

                entity.Property(d => d.RoleId).HasMaxLength(20);
                entity.Property(d => d.Title).HasMaxLength(200).IsRequired();
                entity.Property(d => d.Category).HasMaxLength(100);
                entity.Property(d => d.Description).HasMaxLength(1000);
                entity.Property(d => d.FileType).HasMaxLength(20).IsRequired();
                entity.Property(d => d.FileName).HasMaxLength(300).IsRequired();
                entity.Property(d => d.FilePath).HasMaxLength(500).IsRequired();
                entity.Property(d => d.Length).HasMaxLength(50);
                entity.Property(d => d.UploadedBy).HasMaxLength(10).HasDefaultValue("Rep").IsRequired();
                entity.Property(d => d.UploadedAt).HasDefaultValueSql("SYSUTCDATETIME()");
                entity.Property(d => d.Language).HasDefaultValue("English");

                entity.HasIndex(d => d.RoleId);
            });

            modelBuilder.Entity<TrainingHubLink>(entity =>
            {
                entity.ToTable("TrainingHubLinks");

                entity.HasKey(l => l.OId);

                entity.Property(l => l.ProductVideoEnglishLink).HasMaxLength(500);
                entity.Property(l => l.ProductVideoSpanishLink).HasMaxLength(500);
                entity.Property(l => l.DashboardVideoEnglishLink).HasMaxLength(500);
                entity.Property(l => l.DashboardVideoSpanishLink).HasMaxLength(500);
                entity.Property(l => l.KnowledgeBaseLink).HasMaxLength(500);
                entity.Property(l => l.SalesLeadLink).HasMaxLength(500);
            });
        }
    }
}
