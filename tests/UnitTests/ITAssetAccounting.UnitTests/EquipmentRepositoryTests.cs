using ITAssetAccounting.EquipmentService.Data;
using ITAssetAccounting.EquipmentService.Entities;
using ITAssetAccounting.EquipmentService.Repositories;
using ITAssetAccounting.Shared.Dto;
using ITAssetAccounting.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ITAssetAccounting.UnitTests;

public class EquipmentRepositoryTests
{
    private static EquipmentDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<EquipmentDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new EquipmentDbContext(options);
    }

    [Fact]
    public async Task GetPagedAsync_ShouldFilterByStatus()
    {
        await using var db = CreateContext();
        var category = new Category { Id = 1, Name = "Laptop" };
        var location = new Location { Id = 1, Name = "Stock" };
        await db.Categories.AddAsync(category);
        await db.Locations.AddAsync(location);
        await db.Equipments.AddRangeAsync(
            new EquipmentItem { InventoryNumber = "A-1", Name = "A", CategoryId = 1, LocationId = 1, Status = EquipmentStatus.InStock },
            new EquipmentItem { InventoryNumber = "A-2", Name = "B", CategoryId = 1, LocationId = 1, Status = EquipmentStatus.Assigned });
        await db.SaveChangesAsync();

        var repo = new EquipmentRepository(db);
        var result = await repo.GetPagedAsync(new EquipmentFilter { Status = EquipmentStatus.InStock });

        Assert.Single(result.Items);
        Assert.Equal(EquipmentStatus.InStock, result.Items.First().Status);
    }

    [Fact]
    public async Task GetByInventoryNumberAsync_ShouldReturnExactEquipment()
    {
        await using var db = CreateContext();
        await db.Categories.AddAsync(new Category { Id = 1, Name = "Laptop" });
        await db.Locations.AddAsync(new Location { Id = 1, Name = "Stock" });
        await db.Equipments.AddAsync(new EquipmentItem { InventoryNumber = "NB-100", Name = "ThinkPad", CategoryId = 1, LocationId = 1 });
        await db.SaveChangesAsync();

        var repo = new EquipmentRepository(db);
        var result = await repo.GetByInventoryNumberAsync("NB-100");

        Assert.NotNull(result);
        Assert.Equal("ThinkPad", result!.Name);
    }
}
