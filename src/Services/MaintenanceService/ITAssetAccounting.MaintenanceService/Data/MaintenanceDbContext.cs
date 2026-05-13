using ITAssetAccounting.MaintenanceService.Entities;
using Microsoft.EntityFrameworkCore;

namespace ITAssetAccounting.MaintenanceService.Data;

public class MaintenanceDbContext : DbContext
{
    public MaintenanceDbContext(DbContextOptions<MaintenanceDbContext> options) : base(options) { }

    public DbSet<MaintenanceRequest> MaintenanceRequests => Set<MaintenanceRequest>();
    public DbSet<MaintenanceComment> MaintenanceComments => Set<MaintenanceComment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MaintenanceRequest>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.EquipmentId);
            e.HasIndex(x => x.Status);
            e.Property(x => x.Title).HasMaxLength(160).IsRequired();
            e.Property(x => x.Description).HasMaxLength(2000).IsRequired();
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
            e.Property(x => x.Priority).HasConversion<string>().HasMaxLength(30).IsRequired();
        });

        modelBuilder.Entity<MaintenanceComment>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Text).HasMaxLength(2000).IsRequired();
            e.HasOne(x => x.MaintenanceRequest).WithMany(x => x.Comments).HasForeignKey(x => x.MaintenanceRequestId);
        });
    }
}
