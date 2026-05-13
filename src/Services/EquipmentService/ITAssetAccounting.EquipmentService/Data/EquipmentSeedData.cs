using ITAssetAccounting.EquipmentService.Entities;
using ITAssetAccounting.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace ITAssetAccounting.EquipmentService.Data;

public static class EquipmentSeedData
{
    public static async Task SeedAsync(EquipmentDbContext db)
    {
        if (await db.Categories.AnyAsync())
        {
            return;
        }

        var categories = new[]
        {
            new Category { Name = "Ноутбук", Description = "Переносные рабочие станции" },
            new Category { Name = "Монитор", Description = "Внешние мониторы" },
            new Category { Name = "Принтер", Description = "Печатающие устройства" },
            new Category { Name = "Периферия", Description = "Мыши, клавиатуры, гарнитуры" }
        };
        var locations = new[]
        {
            new Location { Name = "Склад IT", Address = "Главный офис, 1 этаж", Description = "Основной склад оборудования" },
            new Location { Name = "Кабинет 201", Address = "Главный офис, 2 этаж", Description = "Бухгалтерия" },
            new Location { Name = "Кабинет 305", Address = "Главный офис, 3 этаж", Description = "Отдел продаж" }
        };

        await db.Categories.AddRangeAsync(categories);
        await db.Locations.AddRangeAsync(locations);
        await db.SaveChangesAsync();

        var laptop = categories[0];
        var monitor = categories[1];
        var stock = locations[0];
        await db.Equipments.AddRangeAsync(
            new EquipmentItem { InventoryNumber = "NB-0001", Name = "Lenovo ThinkPad", Model = "T14", SerialNumber = "SN-T14-0001", CategoryId = laptop.Id, LocationId = stock.Id, Status = EquipmentStatus.InStock, PurchaseDate = DateTime.UtcNow.AddMonths(-5), Price = 98000 },
            new EquipmentItem { InventoryNumber = "MN-0001", Name = "Dell UltraSharp", Model = "U2722D", SerialNumber = "SN-DELL-0001", CategoryId = monitor.Id, LocationId = stock.Id, Status = EquipmentStatus.InStock, PurchaseDate = DateTime.UtcNow.AddMonths(-8), Price = 36000 });
        await db.SaveChangesAsync();
    }
}
