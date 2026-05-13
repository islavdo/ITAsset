using ITAssetAccounting.FileService.Entities;
using Microsoft.EntityFrameworkCore;

namespace ITAssetAccounting.FileService.Data;

public class FileDbContext : DbContext
{
    public FileDbContext(DbContextOptions<FileDbContext> options) : base(options) { }

    public DbSet<EquipmentFile> EquipmentFiles => Set<EquipmentFile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EquipmentFile>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.EquipmentId);
            e.Property(x => x.OriginalFileName).HasMaxLength(255).IsRequired();
            e.Property(x => x.StoredFileName).HasMaxLength(255).IsRequired();
            e.Property(x => x.ContentType).HasMaxLength(150).IsRequired();
            e.Property(x => x.Kind).HasConversion<string>().HasMaxLength(40).IsRequired();
            e.ToTable(t => t.HasCheckConstraint("CK_File_Size", "Size > 0"));
        });
    }
}
