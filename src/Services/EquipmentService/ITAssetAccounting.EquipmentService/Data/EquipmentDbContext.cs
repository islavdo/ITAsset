using ITAssetAccounting.EquipmentService.Entities;
using ITAssetAccounting.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace ITAssetAccounting.EquipmentService.Data;

public class EquipmentDbContext : DbContext
{
    public EquipmentDbContext(DbContextOptions<EquipmentDbContext> options) : base(options) { }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<EquipmentItem> Equipments => Set<EquipmentItem>();
    public DbSet<Assignment> Assignments => Set<Assignment>();
    public DbSet<EquipmentHistory> EquipmentHistory => Set<EquipmentHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Name).IsUnique();
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<Location>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Name).IsUnique();
            e.Property(x => x.Name).HasMaxLength(120).IsRequired();
        });

        modelBuilder.Entity<EquipmentItem>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.InventoryNumber).IsUnique();
            e.HasIndex(x => x.SerialNumber);
            e.Property(x => x.InventoryNumber).HasMaxLength(80).IsRequired();
            e.Property(x => x.Name).HasMaxLength(160).IsRequired();
            e.Property(x => x.Model).HasMaxLength(120);
            e.Property(x => x.SerialNumber).HasMaxLength(120);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
            e.Property(x => x.Price).HasPrecision(12, 2);
            e.HasOne(x => x.Category).WithMany(x => x.Equipments).HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Location).WithMany(x => x.Equipments).HasForeignKey(x => x.LocationId).OnDelete(DeleteBehavior.Restrict);
            e.ToTable(t => t.HasCheckConstraint("CK_Equipment_Price", "Price IS NULL OR Price >= 0"));
        });

        modelBuilder.Entity<Assignment>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.EquipmentId, x.ReturnedAtUtc });
            e.HasOne(x => x.Equipment).WithMany(x => x.Assignments).HasForeignKey(x => x.EquipmentId);
        });

        modelBuilder.Entity<EquipmentHistory>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.OldStatus).HasConversion<string>();
            e.Property(x => x.NewStatus).HasConversion<string>();
            e.HasOne(x => x.Equipment).WithMany(x => x.History).HasForeignKey(x => x.EquipmentId);
        });
    }
}
