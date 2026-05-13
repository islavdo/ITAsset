using ITAssetAccounting.EquipmentService.Data;

namespace ITAssetAccounting.EquipmentService.Repositories;

public class EquipmentUnitOfWork
{
    private readonly EquipmentDbContext _db;
    public IEquipmentRepository Equipments { get; }

    public EquipmentUnitOfWork(EquipmentDbContext db, IEquipmentRepository equipments)
    {
        _db = db;
        Equipments = equipments;
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
