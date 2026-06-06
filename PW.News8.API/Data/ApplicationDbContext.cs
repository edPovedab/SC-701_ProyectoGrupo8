using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PW.News8.Shared.Models;

namespace PW.News8.API.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Source> Sources { get; set; } = null!;
        public DbSet<SourceItem> SourceItems { get; set; } = null!;
        public DbSet<AppSetting> AppSettings { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ── Sources ───────────────────────────────────────────────
            modelBuilder.Entity<Source>(entity =>
            {
                entity.ToTable("Sources");
                entity.HasKey(s => s.Id);

                entity.Property(s => s.Url)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(s => s.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(s => s.Description)
                    .HasMaxLength(500);

                entity.Property(s => s.ComponentType)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(s => s.RequiresSecret)
                    .HasDefaultValue(false);
            });

            // ── SourceItems ───────────────────────────────────────────
            modelBuilder.Entity<SourceItem>(entity =>
            {
                entity.ToTable("SourceItems");
                entity.HasKey(si => si.Id);

                entity.Property(si => si.Json)
                    .IsRequired()
                    .HasColumnType("nvarchar(max)");

                entity.Property(si => si.CreatedAt)
                    .HasDefaultValueSql("GETDATE()");

                entity.HasOne(si => si.Source)
                    .WithMany(s => s.SourceItems)
                    .HasForeignKey(si => si.SourceId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ── AppSettings ───────────────────────────────────────────
            modelBuilder.Entity<AppSetting>(entity =>
            {
                entity.ToTable("AppSettings");
                entity.HasKey(a => a.Id);

                entity.Property(a => a.Key)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(a => a.Value)
                    .IsRequired()
                    .HasColumnType("nvarchar(max)");

                entity.Property(a => a.Description)
                    .HasMaxLength(300);

                entity.Property(a => a.IsSecret)
                    .HasDefaultValue(false);

                entity.Property(a => a.UpdatedAt)
                    .HasDefaultValueSql("GETDATE()");
            });
        }
    }
}